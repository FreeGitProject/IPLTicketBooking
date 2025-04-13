using System.Collections.Generic;
using IPLTicketBooking.Controllers;
using IPLTicketBooking.DTOs;

namespace IPLTicketBooking.DTOs
{
	public class CategoryWithEventsDto : CategoryResponseDto
	{
		public IEnumerable<EventResponseDto> Events { get; set; }
	}
}