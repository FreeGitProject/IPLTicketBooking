using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace IPLTicketBooking.Models
{
	// Models/Category.cs
	public class Category
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }

		[BsonElement("name")]
		public string Name { get; set; }

		[BsonElement("description")]
		public string Description { get; set; }

		[BsonElement("createdAt")]
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		[BsonElement("updatedAt")]
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
	}
}
