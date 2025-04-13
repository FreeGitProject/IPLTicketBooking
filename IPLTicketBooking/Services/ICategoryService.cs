using IPLTicketBooking.DTOs;
using IPLTicketBooking.Models;

namespace IPLTicketBooking.Services
{
	// Services/ICategoryService.cs
	public interface ICategoryService
	{
		Task<IEnumerable<Category>> GetAllCategoriesAsync();
		Task<Category> GetCategoryByIdAsync(string id);
		Task<Category> GetCategoryByNameAsync(string name);
		Task CreateCategoryAsync(CreateCategoryDto category);
		Task UpdateCategoryAsync(string id, UpdateCategoryDto category);
		Task DeleteCategoryAsync(string id);
	}
}
