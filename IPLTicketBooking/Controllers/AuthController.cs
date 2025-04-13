using Microsoft.AspNetCore.Mvc;
using IPLTicketBooking.DTOs;
using IPLTicketBooking.Services;
using Microsoft.Extensions.Logging;

namespace IPLTicketBooking.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;
		private readonly ILogger<AuthController> _logger;

		public AuthController(IAuthService authService, ILogger<AuthController> logger)
		{
			_authService = authService;
			_logger = logger;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register(RegisterDto registerDto)
		{
			try
			{
				var response = await _authService.Register(registerDto);
				return Ok(response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error during registration");
				return BadRequest(ex.Message);
			}
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login(LoginDto loginDto)
		{
			try
			{
				var response = await _authService.Login(loginDto);
				return Ok(response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error during login");
				return BadRequest(ex.Message);
			}
		}
	}
}