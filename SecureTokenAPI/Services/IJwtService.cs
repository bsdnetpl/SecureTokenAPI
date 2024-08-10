
namespace SecureTokenAPI.Services
    {
    public interface IJwtService
        {
        string GenerateJwtToken(string username, List<string> roles, Guid userToken);
        }
    }