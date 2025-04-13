using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace IPLTicketBooking.Models
{
	public class User
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }

		[BsonElement("username")]
		public string Username { get; set; }

		[BsonElement("email")]
		public string Email { get; set; }

		[BsonElement("passwordHash")]
		public byte[] PasswordHash { get; set; }

		[BsonElement("passwordSalt")]
		public byte[] PasswordSalt { get; set; }

		[BsonElement("roles")]
		public List<string> RoleIds { get; set; } = new List<string>();

		[BsonElement("createdAt")]
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		[BsonElement("updatedAt")]
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
	}
}