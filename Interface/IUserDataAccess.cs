using static CapfortuneBE.Models.AdminUserDTO;

namespace CapfortuneBE.Interface
{
    public interface IUserDataAccess
    {
        Task<AdminUser?> GetUserByMailAsync(string userMail);
        Task<bool> IsUserMailExistsAsync(string userMail);
        Task<AdminUser> RegisterUserAsync(RegisterRequest request);
        Task LogErrorAsync(string layer, string methodName, Exception ex, object? payload = null);
    }
}
