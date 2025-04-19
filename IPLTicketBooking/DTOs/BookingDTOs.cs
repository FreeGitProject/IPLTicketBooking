using System.ComponentModel.DataAnnotations;

namespace IPLTicketBooking.DTOs
{
    public class CreateBookingDto
    {
        [Required]
        public string EventId { get; set; }

        [Required]
        public string HoldId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one seat must be selected")]
        public List<string> SeatIds { get; set; }
    }

    //public class BookingResult
    //{
    //    public bool Success { get; set; }
    //    public string Message { get; set; }
    //    public string BookingId { get; set; }
    //    public decimal TotalAmount { get; set; }
    //    public List<BookedSeatInfo> Seats { get; set; }
    //}

    //public class BookedSeatInfo
    //{
    //    public string SeatId { get; set; }
    //    public string SeatNumber { get; set; }
    //    public string Section { get; set; }
    //    public string Row { get; set; }
    //    public decimal Price { get; set; }
    //}
}
