using CapfortuneBE.Interface;
using Dapper;
using System.Data;
using static CapfortuneBE.Models.AdminUserDTO;

namespace CapfortuneBE.DataAccess
{
    public class UserDataAccess : IUserDataAccess
    {
        private readonly DapperContext _context;
        private readonly ILogger<UserDataAccess> _logger;
        public UserDataAccess(DapperContext context, ILogger<UserDataAccess> logger)
        {
            _context = context;
            _logger = logger;
        }
        private IDbConnection CreateConnection()
        {
            return _context.CreateConnection();
        }
        public async Task<AdminUser?> GetUserByMailAsync(string userMail)
        {
            try
            {
                using var connection = CreateConnection();

                string query = @"
                    SELECT
                        UserId,
                        UserName,
                        UserMail,
                        MobileNumber,
                        Password,
                        Role,
                        IsActive
                    FROM AdminUsers
                    WHERE UserMail = @UserMail
                      AND IsActive = 1";

                return await connection.QuerySingleOrDefaultAsync<AdminUser>(query, new { UserMail = userMail });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving admin user by mail.");
                throw;
            }
        }
        public async Task<bool> IsUserMailExistsAsync(string userMail)
        {
            try
            {
                using var connection = CreateConnection();

                string query = @"
            SELECT COUNT(1)
            FROM AdminUsers
            WHERE UserMail = @UserMail";

                var count = await connection.ExecuteScalarAsync<int>(query, new { UserMail = userMail });
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking user mail existence.");
                throw;
            }
        }

        public async Task<AdminUser> RegisterUserAsync(RegisterRequest request)
        {
            try
            {
                using var connection = CreateConnection();

                string query = @"
            INSERT INTO AdminUsers
            (
                UserName,
                UserMail,
                MobileNumber,
                Password,
                Role,
                IsActive
            )
            VALUES
            (
                @UserName,
                @UserMail,
                @MobileNumber,
                @Password,
                @Role,
                1
            );
            SELECT
                UserId,
                UserName,
                UserMail,
                MobileNumber,
                Password,
                Role,
                IsActive
            FROM AdminUsers
            WHERE UserId = LAST_INSERT_ID();";

                var user = await connection.QuerySingleAsync<AdminUser>(query, new
                {
                    request.UserName,
                    request.UserMail,
                    request.MobileNumber,
                    request.Password,
                    request.Role
                });

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering user.");
                throw;
            }
        }
        public async Task LogErrorAsync(string layer, string methodName, Exception ex, object? payload = null)
        {
            try
            {
                using var connection = CreateConnection();

                string query = @"
                    INSERT INTO ErrorLogs
                    (
                        Layer,
                        MethodName,
                        Message,
                        StackTrace,
                        RequestPayload,
                        InnerException,
                        CreatedDate
                    )
                    VALUES
                    (
                        @Layer,
                        @MethodName,
                        @Message,
                        @StackTrace,
                        @RequestPayload,
                        @InnerException,
                        NOW()
                    )";

                await connection.ExecuteAsync(query, new
                {
                    Layer = layer,
                    MethodName = methodName,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    RequestPayload = payload != null ? System.Text.Json.JsonSerializer.Serialize(payload) : null,
                    InnerException = ex.InnerException?.Message
                });
            }
            catch
            {
                // Silently fail — don't throw from error logger
            }
        }
    }
}
