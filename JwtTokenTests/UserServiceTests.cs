using Microsoft.EntityFrameworkCore;
using Moq;
using SecureTokenAPI.DB;
using SecureTokenAPI.Models;
using SecureTokenAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JwtTokenTests
    {
    public class UserServiceTests : IDisposable
        {
        private readonly UserService _userService;
        private readonly AppDbContext _context;
        private readonly Mock<IJwtService> _jwtServiceMock;

        public UserServiceTests()
            {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new AppDbContext(options);
            _jwtServiceMock = new Mock<IJwtService>();

            _userService = new UserService(_context, _jwtServiceMock.Object);
            }

        // Czyszczenie bazy danych przed każdym testem
        public void Dispose()
            {
            _context.UserTokens.RemoveRange(_context.UserTokens);
            _context.SaveChanges();
            }

        [Fact]
        public async Task RegisterUserAsync_ShouldReturnFalse_WhenUserAlreadyExists()
            {
            // Arrange
            _context.UserTokens.Add(new UserToken { UserName = "existingUser", Email = "existing@example.com" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.RegisterUserAsync("existingUser", "password123", "existing@example.com");

            // Assert
            Assert.False(result);
            }

        [Fact]
        public async Task RegisterUserAsync_ShouldReturnTrue_WhenUserIsRegisteredSuccessfully()
            {
            // Arrange
            // Nie trzeba usuwać użytkowników, Dispose usunie ich przed testem

            // Act
            var result = await _userService.RegisterUserAsync("newUser", "password123", "new@example.com");

            // Assert
            Assert.True(result);
            Assert.Equal(1, await _context.UserTokens.CountAsync());
            }

        [Fact]
        public async Task AuthenticateUserAsync_ShouldReturnNull_WhenUserDoesNotExist()
            {
            // Arrange

            // Act
            var result = await _userService.AuthenticateUserAsync("nonexistent@example.com", "password123");

            // Assert
            Assert.Null(result);
            }

        [Fact]
        public async Task AuthenticateUserAsync_ShouldReturnToken_WhenCredentialsAreValid()
            {
            // Arrange
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");
            var user = new UserToken
                {
                Email = "user@example.com",
                Password = passwordHash,
                Role = "user",
                Token = Guid.NewGuid()
                };

            _context.UserTokens.Add(user);
            await _context.SaveChangesAsync();

            _jwtServiceMock
                .Setup(x => x.GenerateJwtToken(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<Guid>()))
                .Returns("fake-jwt-token");

            // Act
            var result = await _userService.AuthenticateUserAsync("user@example.com", "password123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("fake-jwt-token", result);
            }
        }
    }
