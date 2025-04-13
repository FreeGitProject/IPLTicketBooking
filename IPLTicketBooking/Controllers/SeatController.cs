using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using IPLTicketBooking.Services.IPLTicketBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPLTicketBooking.Controllers
{
	[ApiController]
	[Route("api/events/{eventId}/[controller]")]
	public class SeatsController : ControllerBase
	{
		private readonly ISeatService _seatService;
		private readonly ILogger<SeatsController> _logger;

		public SeatsController(ISeatService seatService, ILogger<SeatsController> logger)
		{
			_seatService = seatService;
			_logger = logger;
		}

		/// <summary>
		/// Get all available seats for an event
		/// </summary>
		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> GetAvailableSeats(string eventId)
		{
			try
			{
				var seats = await _seatService.GetAvailableSeatsAsync(eventId);
				return Ok(seats);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting available seats for event {EventId}", eventId);
				return StatusCode(500, "An error occurred while retrieving available seats");
			}
		}

		/// <summary>
		/// Check specific seat availability
		/// </summary>
		[HttpGet("{seatId}")]
		[AllowAnonymous]
		public async Task<IActionResult> CheckSeatAvailability(string eventId, string seatId)
		{
			try
			{
				var isAvailable = await _seatService.CheckSeatAvailabilityAsync(eventId, seatId);
				return Ok(new { Available = isAvailable });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error checking seat availability for seat {SeatId} in event {EventId}",
					seatId, eventId);
				return StatusCode(500, "An error occurred while checking seat availability");
			}
		}

		/// <summary>
		/// Temporarily hold seats for booking
		/// </summary>
		[HttpPost("hold")]
		//[Authorize]
		public async Task<IActionResult> HoldSeats(string eventId, [FromBody] HoldSeatsRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}
				//var a = new 
				//	var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get user ID from JWT
				var userId = "67fb60163641e0020b08b231";
				var result = await _seatService.HoldSeatsAsync(eventId, request.SeatIds, userId);

				if (!result.Success)
				{
					return BadRequest(new
					{
						result.Message,
						UnavailableSeats = result.UnavailableSeats
					});
				}

				return Ok(new
				{
					result.HoldId,
					result.HeldUntil,
					HeldSeats = result.HeldSeats
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error holding seats for event {EventId}", eventId);
				return StatusCode(500, "An error occurred while holding seats");
			}
		}

		/// <summary>
		/// Confirm seat booking
		/// </summary>
		[HttpPost("book")]
		//[Authorize]
		public async Task<IActionResult> BookSeats(string eventId, [FromBody] BookSeatsRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get user ID from JWT
				var result = await _seatService.BookSeatsAsync(eventId, request.HoldId, request.SeatIds, userId);

				if (!result.Success)
				{
					return BadRequest(result.Message);
				}

				// In a real application, you would:
				// 1. Process payment
				// 2. Send confirmation email
				// 3. Generate tickets

				return Ok(new
				{
					BookingId = result.BookingId,
					TotalAmount = result.TotalAmount
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error booking seats for event {EventId}", eventId);
				return StatusCode(500, "An error occurred while booking seats");
			}
		}
	}

	// DTOs for Seat operations
	public class HoldSeatsRequest
	{
		[Required]
		[MinLength(1, ErrorMessage = "At least one seat must be selected")]
		public List<string> SeatIds { get; set; }
	}

	public class BookSeatsRequest
	{
		[Required]
		public string HoldId { get; set; }

		[Required]
		[MinLength(1, ErrorMessage = "At least one seat must be selected")]
		public List<string> SeatIds { get; set; }
	}
}
