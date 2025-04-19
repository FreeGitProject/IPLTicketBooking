using IPLTicketBooking.Models;
using IPLTicketBooking.Services.IPLTicketBooking.Services;

namespace IPLTicketBooking.Services
{
    public interface IBookingService
    {
        Task<BookingResult> BookSeatsAsync(string eventId, string holdId, List<string> seatIds, string userId);
        Task<BookingResult> ConfirmBookingWithPayment(string bookingId, string paymentId);
        Task<Booking> GetBookingByIdAsync(string bookingId);
        Task<IEnumerable<Booking>> GetUserBookingsAsync(string userId);
        Task<bool> CancelBookingAsync(string bookingId, string userId, bool isAdmin = false);
    }
}
