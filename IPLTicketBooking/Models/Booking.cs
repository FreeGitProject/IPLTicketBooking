using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace IPLTicketBooking.Models
{
	// Models/Booking.cs
	public class Booking
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }

		[BsonElement("userId")]
		public string UserId { get; set; }

		[BsonElement("eventId")]
		[BsonRepresentation(BsonType.ObjectId)]
		public string EventId { get; set; }

		[BsonElement("seats")]
		public List<BookedSeat> Seats { get; set; } = new List<BookedSeat>();

		[BsonElement("totalAmount")]
		public decimal TotalAmount { get; set; }

		[BsonElement("status")]
		public string Status { get; set; } // pending, confirmed, cancelled

		[BsonElement("paymentId")]
		public string PaymentId { get; set; }

		[BsonElement("createdAt")]
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		[BsonElement("updatedAt")]
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        [BsonIgnore]
        public Event Event { get; set; } // Populated when needed
    }

	public class BookedSeat
	{
		[BsonElement("seatId")]
		public string SeatId { get; set; }

		[BsonElement("price")]
		public decimal Price { get; set; }
	}

	// Models/EventSeat.cs
	public class EventSeat
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }

		[BsonElement("eventId")]
		[BsonRepresentation(BsonType.ObjectId)]
		public string EventId { get; set; }

		[BsonElement("seatId")]
		public string SeatId { get; set; }

		[BsonElement("status")]
		public string Status { get; set; } // available, held, booked

		[BsonElement("currentPrice")]
		public decimal CurrentPrice { get; set; }

		[BsonElement("heldUntil")]
		public DateTime? HeldUntil { get; set; }

		[BsonElement("version")]
		public int Version { get; set; } = 1;
	}
}
