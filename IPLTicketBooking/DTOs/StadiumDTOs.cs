using System.ComponentModel.DataAnnotations;

namespace IPLTicketBooking.DTOs
{
	public class CreateStadiumDto
	{
		[Required(ErrorMessage = "Stadium name is required")]
		[StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
		public string Name { get; set; }

		[Required(ErrorMessage = "Location is required")]
		public string Location { get; set; }

		[Required(ErrorMessage = "Capacity is required")]
		[Range(100, 200000, ErrorMessage = "Capacity must be between 100 and 200,000")]
		public int Capacity { get; set; }

		public List<StadiumSectionDto> Sections { get; set; } = new List<StadiumSectionDto>();
	}

	public class UpdateStadiumDto : CreateStadiumDto
	{
		[Required(ErrorMessage = "Stadium ID is required")]
		public string Id { get; set; }
	}

	public class StadiumResponseDto
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Location { get; set; }
		public int Capacity { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public List<StadiumSectionResponseDto> Sections { get; set; }
	}

	public class StadiumSectionDto
	{
		[Required(ErrorMessage = "Section name is required")]
		public string Name { get; set; }
		public string Description { get; set; }
		public List<SeatRowDto> SeatRows { get; set; }
	}

	public class StadiumSectionResponseDto
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
	}

	public class SeatRowDto
	{
		[Required(ErrorMessage = "Row name is required")]
		public string Name { get; set; }
		public List<SeatDto> Seats { get; set; }
	}

	public class SeatDto
	{
		[Required(ErrorMessage = "Seat number is required")]
		public string Number { get; set; }

		[Required(ErrorMessage = "Seat type is required")]
		public string Type { get; set; }

		[Required(ErrorMessage = "X position is required")]
		public int XPosition { get; set; }

		[Required(ErrorMessage = "Y position is required")]
		public int YPosition { get; set; }
	}
	public class StadiumSectionDetailDto
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string StadiumId { get; set; }
		public string StadiumName { get; set; }
		public List<SeatRowDetailDto> SeatRows { get; set; }
	}

	public class SeatRowDetailDto
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public List<SeatDetailDto> Seats { get; set; }
	}

	public class SeatDetailDto
	{
		public string Id { get; set; }
		public string Number { get; set; }
		public string Type { get; set; }
		public int XPosition { get; set; }
		public int YPosition { get; set; }
	}
}
