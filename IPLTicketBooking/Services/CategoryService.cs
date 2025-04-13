using AutoMapper;
using IPLTicketBooking.DTOs;
using IPLTicketBooking.Models;
using IPLTicketBooking.Repositories;

namespace IPLTicketBooking.Services
{
	// Services/CategoryService.cs
	public class CategoryService : ICategoryService
	{
		private readonly ICategoryRepository _categoryRepository;
		private readonly ILogger<CategoryService> _logger;
		private readonly IMapper _mapper;
		public CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger, IMapper mapper)
		{
			_categoryRepository = categoryRepository;
			_logger = logger;
			_mapper = mapper;
		}

		public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
		{
			try
			{
				return await _categoryRepository.GetAllAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting all categories");
				throw;
			}
		}

		public async Task<Category> GetCategoryByIdAsync(string id)
		{
			try
			{
				return await _categoryRepository.GetByIdAsync(id);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting category by ID: {CategoryId}", id);
				throw;
			}
		}

		public async Task<Category> GetCategoryByNameAsync(string name)
		{
			try
			{
				return await _categoryRepository.GetByNameAsync(name);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting category by name: {CategoryName}", name);
				throw;
			}
		}

		public async Task CreateCategoryAsync(CreateCategoryDto categoryDto)
		{
			try
			{
				var category =_mapper.Map<Category>(categoryDto);
				// Check if category with same name already exists
				var existingCategory = await _categoryRepository.GetByNameAsync(category.Name);
				if (existingCategory != null)
				{
					throw new InvalidOperationException($"Category with name '{category.Name}' already exists");
				}

				await _categoryRepository.CreateAsync(category);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating category");
				throw;
			}
		}

		public async Task UpdateCategoryAsync(string id, UpdateCategoryDto categoryDto)
		{
			try
			{
				var category = new Category
				{
					Id = id,
					Name = categoryDto.Name,
					Description = categoryDto.Description,
				};
				//var category = _mapper.Map<Category>(categoryDto);
				
				// Verify the category exists
				var existingCategory = await _categoryRepository.GetByIdAsync(id);
				if (existingCategory == null)
				{
					throw new KeyNotFoundException($"Category with ID '{id}' not found");
				}

				// Check if another category with the same name exists
				var duplicateCategory = await _categoryRepository.GetByNameAsync(category.Name);
				if (duplicateCategory != null && duplicateCategory.Id != id)
				{
					throw new InvalidOperationException($"Another category with name '{category.Name}' already exists");
				}

				await _categoryRepository.UpdateAsync(id, category);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating category with ID: {CategoryId}", id);
				throw;
			}
		}

		public async Task DeleteCategoryAsync(string id)
		{
			try
			{
				// In a real application, you might want to check if there are events associated with this category
				// before allowing deletion

				await _categoryRepository.DeleteAsync(id);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deleting category with ID: {CategoryId}", id);
				throw;
			}
		}
	}
}
