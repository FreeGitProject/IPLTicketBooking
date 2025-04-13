using IPLTicketBooking.DTOs;
using IPLTicketBooking.Models;

namespace IPLTicketBooking.Services
{
	public interface IAuthService
	{
		Task<AuthResponseDto> Register(RegisterDto registerDto);
		Task<AuthResponseDto> Login(LoginDto loginDto);
		string GenerateJwtToken(User user);
	}
}