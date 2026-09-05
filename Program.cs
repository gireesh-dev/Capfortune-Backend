using CapfortuneBE.DataAccess;
using CapfortuneBE.Interface;
using CapfortuneBE.Models;
using CapfortuneBE.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Resend;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Enter your JWT token in the text input below.\nExample: Bearer eyJhbGciOiJIUzI1NiIs..."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(options =>
{
    options.ApiToken = builder.Configuration["ResendSettings:ApiKey"]!;
});

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
    };
});

builder.Services.AddAuthorization();

// AllowedOrigins is supplied per-deployment (environment variables in hosted environments).
// AdditionalOrigins is a separate key so origins committed to source control cannot be
// clobbered by an index collision with Cors__AllowedOrigins__N variables.
var allowedOrigins = (builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
    .Concat(builder.Configuration.GetSection("Cors:AdditionalOrigins").Get<string[]>() ?? Array.Empty<string>())
    .Select(origin => origin.Trim().TrimEnd('/'))
    .Where(origin => origin.Length > 0)
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AppCors", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Local development only: permissive CORS regardless of configured allow-list.
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else if (allowedOrigins.Length > 0)
        {
            // Explicit allow-list: required outside development.
            policy.WithOrigins(allowedOrigins)
                  .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                  .AllowAnyHeader();
        }
        // Otherwise (non-development with no configured origins): policy matches nothing,
        // so all cross-origin requests are rejected until Cors:AllowedOrigins is set.
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // The platform proxy has no fixed address, so the default loopback-only allow-list
    // would discard the headers entirely. ForwardLimit stays at 1 so only the rightmost
    // entry is trusted: the address the edge itself appended, not a client-supplied one.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var rateLimiting = builder.Configuration.GetSection("RateLimiting");
var globalPermitLimit = rateLimiting.GetValue("Global:PermitLimit", 200);
var globalWindowSeconds = rateLimiting.GetValue("Global:WindowSeconds", 60);
var authPermitLimit = rateLimiting.GetValue("Auth:PermitLimit", 5);
var authWindowSeconds = rateLimiting.GetValue("Auth:WindowSeconds", 60);
var enquiryPermitLimit = rateLimiting.GetValue("Enquiry:PermitLimit", 10);
var enquiryWindowSeconds = rateLimiting.GetValue("Enquiry:WindowSeconds", 60);

static string GetClientKey(HttpContext httpContext) =>
    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Baseline limiter applied to every request, keyed by client IP.
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(GetClientKey(httpContext), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = globalPermitLimit,
            Window = TimeSpan.FromSeconds(globalWindowSeconds),
            QueueLimit = 0
        }));

    // Stricter policy for auth endpoints (brute-force protection).
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(GetClientKey(httpContext), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = authPermitLimit,
            Window = TimeSpan.FromSeconds(authWindowSeconds),
            QueueLimit = 0
        }));

    // Stricter policy for the public enquiry form (spam/abuse protection).
    options.AddPolicy("enquiry", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(GetClientKey(httpContext), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = enquiryPermitLimit,
            Window = TimeSpan.FromSeconds(enquiryWindowSeconds),
            QueueLimit = 0
        }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
        }

        context.HttpContext.Response.ContentType = "application/json";
        var body = JsonSerializer.Serialize(new ResponseStatus
        {
            Code = StatusCodes.Status429TooManyRequests,
            Success = false,
            Message = "Too many requests. Please try again later."
        });
        await context.HttpContext.Response.WriteAsync(body, cancellationToken);
    };
});

builder.Services.AddSingleton<DapperContext>();
builder.Services.AddScoped<IEnquiryDataAccess, EnquiryDataAccess>();
builder.Services.AddScoped<IUserDataAccess, UserDataAccess>();
builder.Services.AddScoped<EnquiryService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddTransient<IResend, ResendClient>();
builder.Services.AddScoped<MailService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Railway terminates TLS at its edge proxy, so the real client address arrives in
// X-Forwarded-For. Without this the rate limiter keys every request on the proxy IP,
// making the per-client limits a single shared bucket.
app.UseForwardedHeaders();

app.UseHttpsRedirection();

app.UseCors("AppCors");

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
