using IPLTicketBooking.Models;
using IPLTicketBooking.Repositories;

namespace IPLTicketBooking.Services
{
	public class EventService : IEventService
	{
		private readonly IEventRepository _eventRepository;

		public EventService(IEventRepository eventRepository)
		{
			_eventRepository = eventRepository;
		}

		public async Task<IEnumerable<Event>> GetAllEventsAsync()
		{
			return await _eventRepository.GetAllAsync();
		}

		public async Task<Event> GetEventByIdAsync(string id)
		{
			return await _eventRepository.GetByIdAsync(id);
		}

		public async Task<IEnumerable<Event>> GetEventsByCategoryAsync(string categoryId)
		{
			return await _eventRepository.GetByCategoryAsync(categoryId);
		}

		public async Task CreateEventAsync(Event ev)

	{
			await _eventRepository.CreateAsync(ev);
		}

		public async Task UpdateEventAsync(string id, Event ev)

	{
			await _eventRepository.UpdateAsync(id, ev);
		}

		public async Task DeleteEventAsync(string id)
		{
			await _eventRepository.DeleteAsync(id);
		}
	}
}
