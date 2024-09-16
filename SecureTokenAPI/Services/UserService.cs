using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SecureTokenAPI.DB;
using SecureTokenAPI.Models;

namespace SecureTokenAPI.Services
    {
    public class UserService : IUserService
        {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;

        public UserService(AppDbContext context, IJwtService jwtService)
            {
            _context = context;
            _jwtService = jwtService;
            }

        public async Task<bool> RegisterUserAsync(string username, string password, string email)
            {
            // Tworzenie nowego użytkownika
            var user = new UserToken
                {
                UserName = username,
                Password = password, // Tymczasowo przypisujemy surowe hasło, które później zostanie zwalidowane
                Email = email,
                Role = "user",
                Token = Guid.NewGuid(),
                ExpirationDate = DateTime.UtcNow.AddYears(1)  // Token wygasa po 1 roku
                };

            // Walidacja danych użytkownika
            var validator = new UserTokenValidator();
            var validationResult = validator.Validate(user);

            if (!validationResult.IsValid)
                {
                throw new ValidationException(validationResult.Errors);
                }

            // Hashowanie hasła po walidacji
            user.Password = BCrypt.Net.BCrypt.HashPassword(password);

            // Sprawdź, czy użytkownik o podanej nazwie lub adresie email już istnieje
            if (await _context.UserTokens.AnyAsync(u => u.UserName == username || u.Email == email))
                {
                return false;  // Użytkownik lub adres email już istnieje
                }

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

        public async Task<string?> AuthenticateUserAsync(string email, string password)
            {
            // Wyszukiwanie użytkownika na podstawie adresu e-mail
            var user = await _context.UserTokens.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                return null; // Błędny adres e-mail lub hasło
                }


            var token = _jwtService.GenerateJwtToken(user.Email, new List<string> { user.Role }, user.Token);

            return token;
            }
        public async Task<bool> ChangePasswordAsync(string username, string newPassword)
            {
            // Znajdź użytkownika na podstawie nazwy użytkownika
            var user = await _context.UserTokens.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
                {
                return false; // Nie znaleziono użytkownika
                }

            // Utworzenie obiektu UserToken z nowym hasłem
            var userToken = new UserToken
                {
                UserName = username,
                Password = newPassword,
                Email = user.Email,  // Przypisujemy istniejące dane
                Role = user.Role
                };

            // Walidacja nowego hasła w kontekście całego obiektu UserToken
            var validator = new UserTokenValidator();
            var validationResult = validator.Validate(userToken, options => options.IncludeProperties(x => x.Password));

            if (!validationResult.IsValid)
                {
                throw new ValidationException(validationResult.Errors);
                }

            // Hashowanie nowego hasła
            string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            // Aktualizacja hasła użytkownika
            user.Password = newPasswordHash;
            await _context.SaveChangesAsync();

            return true;
            }
        public async Task<bool> DeleteUserAsync(string username)
            {
            // Znajdź użytkownika na podstawie nazwy użytkownika
            var user = await _context.UserTokens.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
                {
                return false; // Użytkownik nie został znaleziony
                }

            // Usuń użytkownika z bazy danych
            _context.UserTokens.Remove(user);
            await _context.SaveChangesAsync();

            return true;
            }

        }
    }
