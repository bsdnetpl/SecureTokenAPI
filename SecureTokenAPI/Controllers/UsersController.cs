using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NLog;
using SecureTokenAPI.DTO;
using SecureTokenAPI.Services;
using ILogger = NLog.ILogger;

namespace SecureTokenAPI.Controllers
    {
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
        {
        private readonly IUserService _userService;
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public UsersController(IUserService userService)
            {
            _userService = userService;
            }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto userRegisterDto)
            {
            if (userRegisterDto == null)
                {
                _logger.Warn("Invalid user data received during registration.");
                return BadRequest(new { message = "Invalid user data." });
                }

            bool isRegistered = await _userService.RegisterUserAsync(userRegisterDto.UserName, userRegisterDto.Password, userRegisterDto.Email);

            if (!isRegistered)
                {
                _logger.Warn($"Registration failed: Username or email already exists for user {userRegisterDto.UserName}.");
                return BadRequest(new { message = "Username or email already exists." });
                }

            _logger.Info($"User {userRegisterDto.UserName} registered successfully.");
            return Ok(new { message = "User registered successfully" });
            }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userDto)
            {
            var token = await _userService.AuthenticateUserAsync(userDto.Email, userDto.Password);
            if (token == null)
                {
                _logger.Warn($"Login failed for {userDto.Email}. Incorrect email or password.");
                return Unauthorized("Nieprawidłowy adres e-mail lub hasło.");
                }

            _logger.Info($"User {userDto.Email} logged in successfully.");
            return Ok(new { Token = token });
            }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] UserRegisterDto request)
            {
            if (!ModelState.IsValid)
                {
                _logger.Warn("Invalid model state during password change.");
                return BadRequest(ModelState);
                }

            try
                {
                var result = await _userService.ChangePasswordAsync(request.UserName, request.Password);

                if (!result)
                    {
                    _logger.Warn($"Password change failed for user {request.UserName}. User does not exist or password is incorrect.");
                    return BadRequest("Current password is incorrect or user does not exist.");
                    }

                _logger.Info($"Password changed successfully for user {request.UserName}.");
                return Ok("Password changed successfully.");
                }
            catch (ValidationException ex)
                {
                _logger.Error(ex, $"Validation error during password change for user {request.UserName}.");
                return BadRequest(new { Errors = ex.Errors.Select(e => e.ErrorMessage) });
                }
            }
        }
    }
