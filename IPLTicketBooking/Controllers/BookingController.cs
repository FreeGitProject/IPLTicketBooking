using Microsoft.AspNetCore.Mvc;
using IPLTicketBooking.DTOs;
using IPLTicketBooking.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace IPLTicketBooking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IRazorpayService _razorpayService;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(
            IBookingService bookingService,
            IRazorpayService razorpayService,
            ILogger<BookingsController> logger)
        {
            _bookingService = bookingService;
            _razorpayService = razorpayService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto bookingDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var bookingResult = await _bookingService.BookSeatsAsync(
                    bookingDto.EventId,
                    bookingDto.HoldId,
                    bookingDto.SeatIds,
                    userId);

                if (!bookingResult.Success)
                {
                    return BadRequest(bookingResult.Message);
                }

                // Create payment order
                var paymentOrder = await _razorpayService.CreateOrder(new CreatePaymentDto
                {
                    BookingId = bookingResult.BookingId,
                    Amount = bookingResult.TotalAmount,
                    Currency = "INR"
                });

                return Ok(new
                {
                    Booking = bookingResult,
                    PaymentOrder = paymentOrder
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return StatusCode(500, "An error occurred while creating the booking");
            }
        }

        [HttpPost("{bookingId}/confirm-payment")]
        public async Task<IActionResult> ConfirmPayment(string bookingId, [FromBody] VerifyPaymentDto paymentDto)
        {
            try
            {
                // Verify payment first
                var isPaymentValid = await _razorpayService.VerifyPayment(paymentDto);
                if (!isPaymentValid)
                {
                    return BadRequest("Payment verification failed");
                }

                // Confirm booking with payment
                var result = await _bookingService.ConfirmBookingWithPayment(
                    bookingId,
                    paymentDto.RazorpayPaymentId);

                if (!result.Success)
                {
                    return BadRequest(result.Message);
                }

                return Ok(new
                {
                    Success = true,
                    BookingId = bookingId,
                    PaymentId = paymentDto.RazorpayPaymentId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment for booking {BookingId}", bookingId);
                return StatusCode(500, "An error occurred while confirming payment");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBooking(string id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (booking == null)
                {
                    return NotFound();
                }

                // Verify the authenticated user owns this booking
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (booking.UserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                return Ok(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking {BookingId}", id);
                return StatusCode(500, "An error occurred while retrieving the booking");
            }
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetUserBookings()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var bookings = await _bookingService.GetUserBookingsAsync(userId);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for user");
                return StatusCode(500, "An error occurred while retrieving bookings");
            }
        }
        [HttpPost("{bookingId}/cancel")]
        public async Task<IActionResult> CancelBooking(string bookingId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");

                // Admins can cancel any booking, regular users can only cancel their own
                var success = await _bookingService.CancelBookingAsync(bookingId, userId, isAdmin);

                if (!success)
                {
                    return BadRequest("Unable to cancel booking. It may already be cancelled or not found.");
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Booking cancelled successfully",
                    BookingId = bookingId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking {BookingId}", bookingId);
                return StatusCode(500, "An error occurred while cancelling the booking");
            }
        }
    }
}