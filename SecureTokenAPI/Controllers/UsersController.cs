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

        }

    }
