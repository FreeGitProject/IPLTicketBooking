using IPLTicketBooking.Models;

namespace IPLTicketBooking.Services
{
	// Services/ISeatService.cs
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	namespace IPLTicketBooking.Services
	{
		public interface ISeatService
		{
			/// <summary>
			/// Get all available seats for an event (including seats with expired holds)
			/// </summary>
			Task<IEnumerable<AvailableSeatDto>> GetAvailableSeatsAsync(string eventId);

			/// <summary>
			/// Check if a specific seat is available for booking
			/// </summary>
			Task<bool> CheckSeatAvailabilityAsync(string eventId, string seatId);

			/// <summary>
			/// Temporarily hold seats for a user during the booking process
			/// </summary>
			/// <returns>Result with success status, hold details, and any unavailable seats</returns>
			Task<SeatHoldResult> HoldSeatsAsync(string eventId, List<string> seatIds, string userId);

			/// <summary>
			/// Confirm booking for held seats
			/// </summary>
			/// <returns>Result with booking details or failure message</returns>
			Task<BookingResult> BookSeatsAsync(string eventId, string holdId, List<string> seatIds, string userId);

			/// <summary>
			/// Release seats that were held but not booked within the hold period
			/// </summary>
			/// <returns>Number of seats released</returns>
			Task<int> ReleaseExpiredHoldsAsync();

			/// <summary>
			/// Get booking details by ID
			/// </summary>
			Task<Booking> GetBookingByIdAsync(string bookingId);

			/// <summary>
			/// Get all bookings for a specific user
			/// </summary>
			Task<IEnumerable<Booking>> GetUserBookingsAsync(string userId);
		}

		public class AvailableSeatDto
		{
			public string SeatId { get; set; }
			public string EventId { get; set; }
			public decimal Price { get; set; }
			public string Status { get; set; } // "available" or "held"
			public string SeatNumber { get; set; }
			public string SectionName { get; set; }
			public string RowName { get; set; }
			public string SeatType { get; set; }
		}

		public class SeatHoldResult
		{
			public bool Success { get; set; }
			public string Message { get; set; }
			public string HoldId { get; set; }
			public DateTime HeldUntil { get; set; }
			public List<string> HeldSeats { get; set; }
			public List<string> UnavailableSeats { get; set; }
		}

		public class BookingResult
		{
			public bool Success { get; set; }
			public string Message { get; set; }
			public string BookingId { get; set; }
			public decimal TotalAmount { get; set; }
			public DateTime BookingDate { get; set; }
			public List<BookedSeatInfo> BookedSeats { get; set; }
		}

		public class BookedSeatInfo
		{
			public string SeatId { get; set; }
			public string SeatNumber { get; set; }
			public string Section { get; set; }
			public string Row { get; set; }
			public decimal Price { get; set; }
		}
	}
}
