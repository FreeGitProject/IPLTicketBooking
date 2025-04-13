using IPLTicketBooking.Models;

namespace IPLTicketBooking.Repositories
{
	public interface IEventRepository : IMongoRepository<Event>
	{
		Task<IEnumerable<Event>> GetByCategoryAsync(string categoryId);
		Task<IEnumerable<Event>> GetUpcomingEventsAsync();
	}
}
