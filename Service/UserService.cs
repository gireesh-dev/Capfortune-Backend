using CapfortuneBE.Interface;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static CapfortuneBE.Models.AdminUserDTO;

namespace CapfortuneBE.Service
{
    public class UserService
    {
        private readonly IUserDataAccess _userDataAccess;
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _configuration;
        public UserService(IUserDataAccess userDataAccess, ILogger<UserService> logger, IConfiguration configuration)
        {
            _userDataAccess = userDataAccess;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _userDataAccess.GetUserByMailAsync(request.UserMail);

                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                    return null;

                return GenerateToken(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while logging in.");
                throw;
            }
        }
        public async Task<AdminUser?> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var exists = await _userDataAccess.IsUserMailExistsAsync(request.UserMail);
                if (exists)
                    return null;

                request.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

                return await _userDataAccess.RegisterUserAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering user.");
                throw;
            }
        }
        private LoginResponse GenerateToken(AdminUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"]!);
            var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.UserMail),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds
            );

            return new LoginResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                UserName = user.UserName,
                UserMail = user.UserMail,
                Role = user.Role,
                ExpiresAt = expiresAt
            };
        }
    }
}
