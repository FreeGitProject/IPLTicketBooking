using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace IPLTicketBooking.Models
{
	// Models/Stadium.cs
	public class Stadium
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }

		[BsonElement("name")]
		public string Name { get; set; }

		[BsonElement("location")]
		public string Location { get; set; }

		[BsonElement("capacity")]
		public int Capacity { get; set; }

		[BsonElement("sections")]
		public List<StadiumSection> Sections { get; set; } = new List<StadiumSection>();

		[BsonElement("createdAt")]
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		[BsonElement("updatedAt")]
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
	}

	public class StadiumSection
	{
		[BsonElement("id")]
		public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

		[BsonElement("name")]
		public string Name { get; set; }

		[BsonElement("description")]
		public string Description { get; set; }

		[BsonElement("seatRows")]
		public List<SeatRow> SeatRows { get; set; } = new List<SeatRow>();
	}

	public class SeatRow
	{
		[BsonElement("id")]
		public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

		[BsonElement("name")]
		public string Name { get; set; }

		[BsonElement("seats")]
		public List<Seat> Seats { get; set; } = new List<Seat>();
	}

	public class Seat
	{
		[BsonElement("id")]
		public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

		[BsonElement("number")]
		public string Number { get; set; }

		[BsonElement("type")]
		public string Type { get; set; } // standard, premium, vip

		[BsonElement("xPosition")]
		public int XPosition { get; set; }

		[BsonElement("yPosition")]
		public int YPosition { get; set; }
	}
}
