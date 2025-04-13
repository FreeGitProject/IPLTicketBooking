using IPLTicketBooking.Models;
using IPLTicketBooking.Services;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace IPLTicketBooking.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize(Roles = "Admin,Organizer")] // Secure endpoints
	public class EventsController : ControllerBase
	{
		private readonly IEventService _eventService;
		private readonly ILogger<EventsController> _logger;

		public EventsController(IEventService eventService, ILogger<EventsController> logger)
		{
			_eventService = eventService;
			_logger = logger;
		}

		/// <summary>
		/// Get all events (filterable by category)
		/// </summary>
		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> GetAll([FromQuery] string categoryId = null)
		{
			try
			{
				IEnumerable<Event> events;

				if (!string.IsNullOrEmpty(categoryId))
				{
					events = await _eventService.GetEventsByCategoryAsync(categoryId);
				}
				else
				{
					events = await _eventService.GetAllEventsAsync();
				}

				return Ok(events.Select(e => new EventResponseDto
				{
					Id = e.Id,
					Name = e.Name,
					CategoryId = e.CategoryId,
					StadiumId = e.StadiumId,
					DateTime = e.DateTime,
					Duration = e.Duration,
					Description = e.Description,
					BasePrice = e.BasePrice,
					Status = e.Status
				}));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting events");
				return StatusCode(500, "An error occurred while retrieving events");
			}
		}

		/// <summary>
		/// Get event by ID
		/// </summary>
		[HttpGet("{id}")]
		[AllowAnonymous]
		public async Task<IActionResult> GetById(string id)
		{
			try
			{
				var ev = await _eventService.GetEventByIdAsync(id);
            if (ev == null)

			{
			return NotFound();
		}

            return Ok(new EventResponseDto
            {
                Id = ev.Id,
                Name = ev.Name,
                CategoryId = ev.CategoryId,
                StadiumId = ev.StadiumId,
                DateTime = ev.DateTime,
                Duration = ev.Duration,
                Description = ev.Description,
                BasePrice = ev.BasePrice,
                Status = ev.Status
		});
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event with ID: {EventId}", id);
            return StatusCode(500, "An error occurred while retrieving the event");
}
    }

    /// <summary>
    /// Create a new event
    /// </summary>
    [HttpPost]
public async Task<IActionResult> Create([FromBody] CreateEventDto eventDto)
{
	try
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		var newEvent = new Event
		{
			Name = eventDto.Name,
			CategoryId = eventDto.CategoryId,
			StadiumId = eventDto.StadiumId,
			DateTime = eventDto.DateTime,
			Duration = eventDto.Duration,
			Description = eventDto.Description,
			BasePrice = eventDto.BasePrice,
			Status = "upcoming" // Default status
		};

		await _eventService.CreateEventAsync(newEvent);

		return CreatedAtAction(nameof(GetById), new { id = newEvent.Id }, new EventResponseDto
		{
			Id = newEvent.Id,
			Name = newEvent.Name,
			CategoryId = newEvent.CategoryId,
			StadiumId = newEvent.StadiumId,
			DateTime = newEvent.DateTime,
			Duration = newEvent.Duration,
			Description = newEvent.Description,
			BasePrice = newEvent.BasePrice,
			Status = newEvent.Status
		});
	}
	catch (Exception ex)
	{
		_logger.LogError(ex, "Error creating event");
		return StatusCode(500, "An error occurred while creating the event");
	}
}

/// <summary>
/// Update an existing event
/// </summary>
[HttpPut("{id}")]
public async Task<IActionResult> Update(string id, [FromBody] UpdateEventDto eventDto)
{
	try
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		var existingEvent = await _eventService.GetEventByIdAsync(id);
		if (existingEvent == null)
		{
			return NotFound();
		}

		existingEvent.Name = eventDto.Name;
		existingEvent.CategoryId = eventDto.CategoryId;
		existingEvent.StadiumId = eventDto.StadiumId;
		existingEvent.DateTime = eventDto.DateTime;
		existingEvent.Duration = eventDto.Duration;
		existingEvent.Description = eventDto.Description;
		existingEvent.BasePrice = eventDto.BasePrice;
		existingEvent.Status = eventDto.Status;
		existingEvent.UpdatedAt = DateTime.UtcNow;

		await _eventService.UpdateEventAsync(id, existingEvent);

		return NoContent();
	}
	catch (Exception ex)
	{
		_logger.LogError(ex, "Error updating event with ID: {EventId}", id);
		return StatusCode(500, "An error occurred while updating the event");
	}
}

/// <summary>
/// Cancel an event
/// </summary>
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(string id)
{
	try
	{
		var existingEvent = await _eventService.GetEventByIdAsync(id);
		if (existingEvent == null)
		{
			return NotFound();
		}

		// In a real application, you would also need to handle:
		// 1. Refunds for existing bookings
		// 2. Notifications to users
		existingEvent.Status = "cancelled";
		existingEvent.UpdatedAt = DateTime.UtcNow;

		await _eventService.UpdateEventAsync(id, existingEvent);

		return NoContent();
	}
	catch (Exception ex)
	{
		_logger.LogError(ex, "Error cancelling event with ID: {EventId}", id);
		return StatusCode(500, "An error occurred while cancelling the event");
	}
}
}

// DTOs for Event
public class CreateEventDto
{
	[Required]
	public string Name { get; set; }

	[Required]
	public string CategoryId { get; set; }

	[Required]
	public string StadiumId { get; set; }

	[Required]
	public DateTime DateTime { get; set; }

	[Range(30, 600)]
	public int Duration { get; set; } // in minutes

	public string Description { get; set; }

	[Range(0, double.MaxValue)]
	public decimal BasePrice { get; set; }
}

public class UpdateEventDto : CreateEventDto
{
	[Required]
	public string Status { get; set; } // upcoming, ongoing, completed, cancelled
}

public class EventResponseDto
{
	public string Id { get; set; }
	public string Name { get; set; }
	public string CategoryId { get; set; }
	public string StadiumId { get; set; }
	public DateTime DateTime { get; set; }
	public int Duration { get; set; }
	public string Description { get; set; }
	public decimal BasePrice { get; set; }
	public string Status { get; set; }
}
}
