namespace IPLTicketBooking.Models
{
    public class Payment
    {
        public string Id { get; set; }
        public string BookingId { get; set; }
        public string RazorpayPaymentId { get; set; }
        public string RazorpayOrderId { get; set; }
        public string RazorpaySignature { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; } // created, captured, failed
        public DateTime CreatedAt { get; set; }
    }


    public class RazorpayOrder
    {
        public string Id { get; set; }
        public string Entity { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
    }
}
