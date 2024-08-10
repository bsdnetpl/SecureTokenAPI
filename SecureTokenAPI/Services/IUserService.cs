using SecureTokenAPI.Models;

namespace SecureTokenAPI.Services
    {
    public interface IUserService
        {
        Task<string?> AuthenticateUserAsync(string userName, string password);
        Task<List<UserToken>> GetAllUsersAsync();
        Task<UserToken> GetUserByUsernameAsync(string username);
        Task<bool> RegisterUserAsync(string username, string password, string email);
        }
    }