using Razorpay.Api;
using IPLTicketBooking.Models;
using Microsoft.Extensions.Options;
using IPLTicketBooking.DTOs;
using IPLTicketBooking.Repositories;
using System.Security.Cryptography;
using System.Text;


namespace IPLTicketBooking.Services
{


    public class RazorpayService : IRazorpayService
    {
        private readonly RazorpayClient _client;
        private readonly ILogger<RazorpayService> _logger;
        private readonly string _razorpayKey;
        private readonly string _razorpayKeySecret;
        private readonly IPaymentRepository _paymentRepository;

        public RazorpayService(
            IOptions<RazorpaySettings> razorpaySettings,
            ILogger<RazorpayService> logger,
            IPaymentRepository paymentRepository)
        {
            _razorpayKeySecret = razorpaySettings.Value.KeySecret;
            _razorpayKey = razorpaySettings.Value.KeyId;
            _client = new RazorpayClient(
                razorpaySettings.Value.KeyId,
                razorpaySettings.Value.KeySecret);
            _logger = logger;
            _paymentRepository = paymentRepository;
        }


        public async Task<PaymentResponseDto> CreateOrder(CreatePaymentDto paymentDto)
        {
            try
            {
                var options = new Dictionary<string, object>
                {
                    { "amount", paymentDto.Amount * 100 }, // Razorpay uses paise
                    { "currency", paymentDto.Currency },
                    { "receipt", $"booking_{paymentDto.BookingId}" },
                    { "payment_capture", 1 } // Auto-capture payment
                };


                var order = await Task.Run(() => _client.Order.Create(options));

                // Store payment record
                var payment = new Models.Payment
                {
                    BookingId = paymentDto.BookingId,
                    RazorpayOrderId = order["id"].ToString(),
                    Amount = paymentDto.Amount,
                    Currency = paymentDto.Currency,
                    Status = "created",
                    CreatedAt = DateTime.UtcNow
                };
                await _paymentRepository.CreateAsync(payment);
                return new PaymentResponseDto
                {
                    OrderId = order["id"].ToString(),
                    RazorpayKey = _razorpayKey,
                    Amount = paymentDto.Amount,
                    Currency = paymentDto.Currency,
                    BookingId = paymentDto.BookingId,
                    PaymentId = payment.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Razorpay order");
                throw;
            }
        }

        public async Task<bool> VerifyPayment(VerifyPaymentDto paymentDto)
        {
            try
            {
                Dictionary<string, string> attributes = new Dictionary<string, string>
        {
            { "razorpay_payment_id", paymentDto.RazorpayPaymentId },
            { "razorpay_order_id", paymentDto.RazorpayOrderId },
            { "razorpay_signature", paymentDto.RazorpaySignature }
        };

                string secret = _razorpayKeySecret;

                // This is the correct way to access Utility in newer versions
                Utils.verifyPaymentSignature(attributes);

                return true;
            }
            catch (Razorpay.Api.Errors.SignatureVerificationError)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying payment signature");
                return false;
            }
        }
        //public async Task<bool> VerifyPayment(VerifyPaymentDto paymentDto)
        //{
        //    try
        //    {
        //        // Verify signature
        //        var isValid = await Task.Run(() =>
        //            _client.Utility.VerifyPaymentSignature(new Dictionary<string, string>
        //            {
        //        { "razorpay_payment_id", paymentDto.RazorpayPaymentId },
        //        { "razorpay_order_id", paymentDto.RazorpayOrderId },
        //        { "razorpay_signature", paymentDto.RazorpaySignature }
        //            }));

        //        if (isValid)
        //        {
        //            // Update payment record
        //            var payment = await _paymentRepository.GetByRazorpayPaymentIdAsync(paymentDto.RazorpayPaymentId);
        //            if (payment != null)
        //            {
        //                payment.RazorpayPaymentId = paymentDto.RazorpayPaymentId;
        //                payment.RazorpaySignature = paymentDto.RazorpaySignature;
        //                payment.Status = "captured";
        //                await _paymentRepository.UpdateAsync(payment.Id, payment);
        //            }
        //        }

        //        return isValid;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error verifying payment");
        //        return false;
        //    }
        //}
    }
}
