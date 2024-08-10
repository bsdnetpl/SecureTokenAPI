using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SecureTokenAPI.Models;
using SecureTokenAPI.Services;

namespace SecureTokenAPI.Controllers
    {
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors]
    public class AdminController : ControllerBase
        {
        private readonly IUserService _userService;

        public AdminController(IUserService userService)
            {
            _userService = userService;
            }

        // Endpoint do pobierania listy wszystkich użytkowników
        [HttpGet("users")]
        public async Task<ActionResult<List<UserToken>>> GetAllUsers()
            {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
            }
        }
    }
