using Microsoft.EntityFrameworkCore;
using SecureTokenAPI.DB;
using SecureTokenAPI.Models;

namespace SecureTokenAPI.Services
    {
    public class UserService
        {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
            {
            _context = context;
            }

        public async Task<bool> RegisterUserAsync(string username, string password, string email)
            {
            // Sprawdź, czy użytkownik o podanej nazwie lub adresie email już istnieje
            if (await _context.UserTokens.AnyAsync(u => u.UserName == username || u.Email == email))
                {
                return false;  // Użytkownik lub adres email już istnieje
                }

            // Hashowanie hasła
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            // Tworzenie nowego użytkownika
            var user = new UserToken
                {
                UserName = username,
                Password = passwordHash,
                Email = email,
                Token = Guid.NewGuid(),
                ExpirationDate = DateTime.UtcNow.AddYears(1)  // Token wygasa po 1 roku
                };

            // Dodanie użytkownika do bazy danych
            _context.UserTokens.Add(user);
            await _context.SaveChangesAsync();

            return true;
            }

        public async Task<UserToken> GetUserByUsernameAsync(string username)
            {
            return await _context.UserTokens.FirstOrDefaultAsync(u => u.UserName == username);
            }

        public async Task<List<UserToken>> GetAllUsersAsync()
            {
            return await _context.UserTokens.ToListAsync();
            }
        }
    }
