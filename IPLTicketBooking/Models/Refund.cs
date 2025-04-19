namespace IPLTicketBooking.Models
{
    public class Refund
    {
        public string Id { get; set; }
        public string BookingId { get; set; }
        public string PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; } // initiated, processed, failed
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}
