using Microsoft.AspNetCore.Mvc;
using IPLTicketBooking.DTOs;
using IPLTicketBooking.Services;
using Microsoft.AspNetCore.Authorization;


namespace IPLTicketBooking.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IRazorpayService _razorpayService;
        private readonly IBookingService _bookingService;
        private readonly ILogger<PaymentsController> _logger;


        public PaymentsController(
            IRazorpayService razorpayService,
            IBookingService bookingService,
            ILogger<PaymentsController> logger)
        {
            _razorpayService = razorpayService;
            _bookingService = bookingService;
            _logger = logger;
        }


        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreatePaymentDto paymentDto)
        {
            try
            {
                var response = await _razorpayService.CreateOrder(paymentDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment order");
                return BadRequest("Error creating payment order");
            }
        }


        [HttpPost("verify")]
        public async Task<IActionResult> VerifyPayment([FromBody] VerifyPaymentDto paymentDto)
        {
            try
            {
                var isValid = await _razorpayService.VerifyPayment(paymentDto);
                if (!isValid)
                {
                    return BadRequest("Payment verification failed");
                }


                // Update booking with payment confirmation
                var result = await _bookingService.ConfirmBookingWithPayment(
                    paymentDto.BookingId,
                    paymentDto.RazorpayPaymentId);


                if (!result.Success)
                {
                    return BadRequest("Failed to confirm booking");
                }


                return Ok(new { success = true, bookingId = paymentDto.BookingId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying payment");
                return StatusCode(500, "Error processing payment");
            }
        }
    }
}
