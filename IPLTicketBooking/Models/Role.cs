using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace IPLTicketBooking.Models
{
	public class Role
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }

		[BsonElement("name")]
		public string Name { get; set; } // Admin, Organizer, User

		[BsonElement("permissions")]
		public List<string> Permissions { get; set; } = new List<string>();

		[BsonElement("createdAt")]
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}