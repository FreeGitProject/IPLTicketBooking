using IPLTicketBooking.DTOs;

namespace IPLTicketBooking.Services
{
	public interface IStadiumService
	{
		Task<IEnumerable<StadiumResponseDto>> GetAllStadiumsAsync();
		Task<StadiumResponseDto> GetStadiumByIdAsync(string id);
		Task<StadiumResponseDto> CreateStadiumAsync(CreateStadiumDto stadiumDto);
		Task UpdateStadiumAsync(string id, UpdateStadiumDto stadiumDto);
		Task DeleteStadiumAsync(string id);
		Task<StadiumSectionDetailDto> GetStadiumSectionDetailAsync(string stadiumId, string sectionId);
	}
}
