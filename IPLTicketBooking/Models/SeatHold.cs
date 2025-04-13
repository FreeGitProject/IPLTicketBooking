using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace IPLTicketBooking.Models
{
	public class SeatHold
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }
		public string EventId { get; set; }
		public List<string> SeatIds { get; set; }
		public string UserId { get; set; }
		public DateTime HeldUntil { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}
