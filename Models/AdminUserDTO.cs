namespace CapfortuneBE.Models
{
    public class AdminUserDTO
    {
        public class LoginRequest
        {
            public string UserMail { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
            public string UserMail { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public DateTime ExpiresAt { get; set; }
        }
        public class AdminUser
        {
            public int UserId { get; set; }
            public string UserName { get; set; } = string.Empty;
            public string UserMail { get; set; } = string.Empty;
            public string MobileNumber { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public bool IsActive { get; set; }
        }
        public class RegisterRequest
        {
            public string UserName { get; set; } = string.Empty;
            public string UserMail { get; set; } = string.Empty;
            public string MobileNumber { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }
    }
}
