using IPLTicketBooking.DTOs;


namespace IPLTicketBooking.Services
{
    public interface IRazorpayService
    {
        Task<PaymentResponseDto> CreateOrder(CreatePaymentDto paymentDto);
        Task<bool> VerifyPayment(VerifyPaymentDto paymentDto);
    }
}
