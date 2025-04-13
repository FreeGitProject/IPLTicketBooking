using IPLTicketBooking.Models;

namespace IPLTicketBooking.Services
{
	// Services/IEventService.cs
	public interface IEventService
	{
		Task<IEnumerable<Event>> GetAllEventsAsync();
		Task<Event> GetEventByIdAsync(string id);
		Task<IEnumerable<Event>> GetEventsByCategoryAsync(string categoryId);
		Task CreateEventAsync(Event ev);

		Task UpdateEventAsync(string id, Event ev);

		Task DeleteEventAsync(string id);
}

}
