using IPLTicketBooking.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Razorpay.Api;

namespace IPLTicketBooking.Utilities
{
	// Utilities/MongoDBContext.cs
	// Utilities/MongoDBContext.cs
	public class MongoDBContext
	{
		private readonly IMongoDatabase _database;

		public MongoDBContext(IOptions<MongoDBSettings> settings)
		{
			var client = new MongoClient(settings.Value.ConnectionString);
			_database = client.GetDatabase(settings.Value.DatabaseName);
		}

		public IMongoCollection<Category> Categories => _database.GetCollection<Category>("Categories");
		public IMongoCollection<Event> Events => _database.GetCollection<Event>("Events");
		public IMongoCollection<Stadium> Stadiums => _database.GetCollection<Stadium>("Stadiums");
		public IMongoCollection<Booking> Bookings => _database.GetCollection<Booking>("Bookings");
		public IMongoCollection<EventSeat> EventSeats => _database.GetCollection<EventSeat>("EventSeats");

		// If using separate collection for seat holds
		public IMongoCollection<SeatHold> SeatHolds => _database.GetCollection<SeatHold>("SeatHolds");
		public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
		public IMongoCollection<Role> Roles => _database.GetCollection<Role>("Roles");
		public IMongoCollection<Models.Payment> Payments => _database.GetCollection<Models.Payment>("Payments");
        public IMongoCollection<Models.Refund> Refunds => _database.GetCollection<Models.Refund>("Refunds");
    }
}
