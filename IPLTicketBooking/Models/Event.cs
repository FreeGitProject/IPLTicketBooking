using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace IPLTicketBooking.Models
{
	// Models/Event.cs
	public class Event
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }

		[BsonElement("name")]
		public string Name { get; set; }

		[BsonElement("categoryId")]
		[BsonRepresentation(BsonType.ObjectId)]
		public string CategoryId { get; set; }

		[BsonElement("stadiumId")]
		[BsonRepresentation(BsonType.ObjectId)]
		public string StadiumId { get; set; }

		[BsonElement("dateTime")]
		public DateTime DateTime { get; set; }

		[BsonElement("duration")]
		public int Duration { get; set; } // in minutes

		[BsonElement("description")]
		public string Description { get; set; }

		[BsonElement("basePrice")]
		public decimal BasePrice { get; set; }

		[BsonElement("status")]
		public string Status { get; set; } // upcoming, ongoing, completed, cancelled

		[BsonElement("createdAt")]
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		[BsonElement("updatedAt")]
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
	}
}
