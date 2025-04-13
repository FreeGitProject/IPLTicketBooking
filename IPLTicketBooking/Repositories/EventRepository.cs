using IPLTicketBooking.Models;
using MongoDB.Driver;

namespace IPLTicketBooking.Repositories
{
	// Repositories/EventRepository.cs
	public class EventRepository : MongoRepository<Event>, IEventRepository
	{
		public EventRepository(IMongoCollection<Event> collection) : base(collection)
		{
			
		}

		public async Task<IEnumerable<Event>> GetByCategoryAsync(string categoryId)
		{
			var filter = Builders<Event>.Filter.Eq("categoryId", categoryId);
			return await _collection.Find(filter).ToListAsync();
		}

		public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
		{
			var filter = Builders<Event>.Filter.Gte("dateTime", DateTime.UtcNow) &
						Builders<Event>.Filter.Eq("status", "upcoming");
			return await _collection.Find(filter).ToListAsync();
		}
	}
}
