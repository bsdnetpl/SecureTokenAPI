using SecureTokenAPI.Models;

namespace SecureTokenAPI.Services
    {
    public interface IUserService
        {
        Task<string?> AuthenticateUserAsync(string email, string password);
        Task<bool> ChangePasswordAsync(string username, string newPassword);
        Task<bool> DeleteUserAsync(string username);
        Task<List<UserToken>> GetAllUsersAsync();
        Task<UserToken> GetUserByUsernameAsync(string username);
        Task<bool> RegisterUserAsync(string username, string password, string email);
        }
    }