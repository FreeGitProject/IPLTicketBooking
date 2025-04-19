namespace IPLTicketBooking.DTOs
{
    public class CreatePaymentDto
    {
        public string BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
    }


    public class PaymentResponseDto
    {
        public string OrderId { get; set; }
        public string RazorpayKey { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string BookingId { get; set; }
        public string PaymentId { get; set; } // Added this
    }


    public class VerifyPaymentDto
    {
        public string RazorpayPaymentId { get; set; }
        public string RazorpayOrderId { get; set; }
        public string RazorpaySignature { get; set; }
        public string BookingId { get; set; }
    }
}
