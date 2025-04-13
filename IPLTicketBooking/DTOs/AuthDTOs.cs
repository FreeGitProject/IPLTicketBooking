using System.ComponentModel.DataAnnotations;

namespace IPLTicketBooking.DTOs
{
	public class RegisterDto
	{
		[Required]
		[StringLength(50, MinimumLength = 3)]
		public string Username { get; set; }

		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		[StringLength(100, MinimumLength = 6)]
		public string Password { get; set; }
	}

	public class LoginDto
	{
		[Required]
		public string Username { get; set; }

		[Required]
		public string Password { get; set; }
	}

	public class AuthResponseDto
	{
		public string Id { get; set; }
		public string Username { get; set; }
		public string Email { get; set; }
		public string Token { get; set; }
		public List<string> Roles { get; set; }
	}
}