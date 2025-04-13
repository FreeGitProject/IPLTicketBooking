using System.ComponentModel.DataAnnotations;

namespace IPLTicketBooking.DTOs
{
	public class CreateCategoryDto
	{
		[Required(ErrorMessage = "Category name is required")]
		[StringLength(100, MinimumLength = 3, ErrorMessage = "Category name must be between 3 and 100 characters")]
		public string Name { get; set; }

		[StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
		public string Description { get; set; }
	}
}