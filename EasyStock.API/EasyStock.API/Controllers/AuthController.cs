using EasyStock.API.Dtos;
using EasyStock.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EasyStock.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) 
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var token = await _authService.AuthenticateAsync(dto.UserName, dto.Password);
            if (token == null)
                return Unauthorized();

            return Ok(new { token });
        }
    }
}
