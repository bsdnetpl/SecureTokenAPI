using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SecureTokenAPI.DTO;
using SecureTokenAPI.Services;

namespace SecureTokenAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
        {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
            {
            _userService = userService;
            }

        // Endpoint do rejestracji nowego użytkownika
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto userRegisterDto)
            {
            if (userRegisterDto == null)
                {
                return BadRequest(new { message = "Invalid user data." });
                }

            bool isRegistered = await _userService.RegisterUserAsync(userRegisterDto.UserName, userRegisterDto.Password, userRegisterDto.Email);

            if (!isRegistered)
                {
                return BadRequest(new { message = "Username or email already exists." });
                }

            return Ok(new { message = "User registered successfully" });
            }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userDto)
            {
            var token = await _userService.AuthenticateUserAsync(userDto.Email, userDto.Password);
            if (token == null)
                {
                return Unauthorized("Nieprawidłowy adres e-mail lub hasło.");
                }

            return Ok(new { Token = token });
            }
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] UserRegisterDto request)
            {
            if (!ModelState.IsValid)
                {
                return BadRequest(ModelState);
                }

            try
                {
                var result = await _userService.ChangePasswordAsync(request.UserName, request.Password);

                if (!result)
                    {
                    return BadRequest("Current password is incorrect or user does not exist.");
                    }

                return Ok("Password changed successfully.");
                }
            catch (ValidationException ex)
                {
                return BadRequest(new { Errors = ex.Errors.Select(e => e.ErrorMessage) });
                }
            }

        }

    }
