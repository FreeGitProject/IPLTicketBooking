using IPLTicketBooking.DTOs;
using IPLTicketBooking.Models;
using IPLTicketBooking.Services;
using Microsoft.AspNetCore.Mvc;

namespace IPLTicketBooking.Controllers
{
	// Controllers/CategoriesController.cs
	[ApiController]
	[Route("api/[controller]")]
	public class CategoriesController : ControllerBase
	{
		private readonly ICategoryService _categoryService;

		public CategoriesController(ICategoryService categoryService)
		{
			_categoryService = categoryService;
		}

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var categories = await _categoryService.GetAllCategoriesAsync();
			return Ok(categories);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(string id)
		{
			var category = await _categoryService.GetCategoryByIdAsync(id);
			if (category == null)
			{
				return NotFound();
			}
			return Ok(category);
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CreateCategoryDto categoryDto)
		{
			
			await _categoryService.CreateCategoryAsync(categoryDto);
			
			//return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
			return Ok();	
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(string id, [FromBody] UpdateCategoryDto category)
		{
			//if (id != category.Id)
			//{
			//	return BadRequest();
			//}

			await _categoryService.UpdateCategoryAsync(id, category);
			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(string id)
		{
			await _categoryService.DeleteCategoryAsync(id);
			return NoContent();
		}
	}
}
