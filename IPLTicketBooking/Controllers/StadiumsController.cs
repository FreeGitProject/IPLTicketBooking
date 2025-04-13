using IPLTicketBooking.DTOs;
using IPLTicketBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPLTicketBooking.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	
	public class StadiumsController : ControllerBase
	{
		private readonly IStadiumService _stadiumService;
		private readonly ILogger<StadiumsController> _logger;

		public StadiumsController(IStadiumService stadiumService, ILogger<StadiumsController> logger)
		{
			_stadiumService = stadiumService;
			_logger = logger;
		}

		[HttpGet]
		[AllowAnonymous]
		public async Task<IActionResult> GetAll()
		{
			try
			{
				var stadiums = await _stadiumService.GetAllStadiumsAsync();
				return Ok(stadiums);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting all stadiums");
				return StatusCode(500, "An error occurred while retrieving stadiums");
			}
		}

		[HttpGet("{id}")]
		[AllowAnonymous]
		public async Task<IActionResult> GetById(string id)
		{
			try
			{
				var stadium = await _stadiumService.GetStadiumByIdAsync(id);
				if (stadium == null)
				{
					return NotFound();
				}
				return Ok(stadium);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting stadium with ID: {StadiumId}", id);
				return StatusCode(500, "An error occurred while retrieving the stadium");
			}
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Create([FromBody] CreateStadiumDto stadiumDto)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var stadium = await _stadiumService.CreateStadiumAsync(stadiumDto);
				return CreatedAtAction(nameof(GetById), new { id = stadium.Id }, stadium);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating stadium");
				return StatusCode(500, "An error occurred while creating the stadium");
			}
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(string id, [FromBody] UpdateStadiumDto stadiumDto)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				if (id != stadiumDto.Id)
				{
					return BadRequest("ID mismatch");
				}

				await _stadiumService.UpdateStadiumAsync(id, stadiumDto);
				return NoContent();
			}
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating stadium with ID: {StadiumId}", id);
				return StatusCode(500, "An error occurred while updating the stadium");
			}
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(string id)
		{
			try
			{
				await _stadiumService.DeleteStadiumAsync(id);
				return NoContent();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deleting stadium with ID: {StadiumId}", id);
				return StatusCode(500, "An error occurred while deleting the stadium");
			}
		}
		[HttpGet("{stadiumId}/Sections/{sectionId}")]
		[AllowAnonymous]
		public async Task<IActionResult> GetSectionDetail(string stadiumId, string sectionId)
		{
			try
			{
				var section = await _stadiumService.GetStadiumSectionDetailAsync(stadiumId, sectionId);
				if (section == null)
				{
					return NotFound();
				}
				return Ok(section);
			}
			catch (KeyNotFoundException ex)
			{
				return NotFound(ex.Message);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting section detail for stadium {StadiumId}, section {SectionId}",
					stadiumId, sectionId);
				return StatusCode(500, "An error occurred while retrieving the section details");
			}
		}

	}
}
