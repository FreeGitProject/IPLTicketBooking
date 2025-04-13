using AutoMapper;
using IPLTicketBooking.DTOs;
using IPLTicketBooking.Models;
using IPLTicketBooking.Repositories;

namespace IPLTicketBooking.Services
{
	public class StadiumService : IStadiumService
	{
		private readonly IStadiumRepository _stadiumRepository;
		private readonly IMapper _mapper;
		private readonly ILogger<StadiumService> _logger;

		public StadiumService(
			IStadiumRepository stadiumRepository,
			IMapper mapper,
			ILogger<StadiumService> logger)
		{
			_stadiumRepository = stadiumRepository;
			_mapper = mapper;
			_logger = logger;
		}

		public async Task<IEnumerable<StadiumResponseDto>> GetAllStadiumsAsync()
		{
			try
			{
				var stadiums = await _stadiumRepository.GetAllAsync();
				return _mapper.Map<IEnumerable<StadiumResponseDto>>(stadiums);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting all stadiums");
				throw;
			}
		}

		public async Task<StadiumResponseDto> GetStadiumByIdAsync(string id)
		{
			try
			{
				var stadium = await _stadiumRepository.GetByIdAsync(id);
				return _mapper.Map<StadiumResponseDto>(stadium);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting stadium with ID: {StadiumId}", id);
				throw;
			}
		}

		public async Task<StadiumResponseDto> CreateStadiumAsync(CreateStadiumDto stadiumDto)
		{
			try
			{
				var existingStadium = await _stadiumRepository.GetByNameAsync(stadiumDto.Name);
				if (existingStadium != null)
				{
					throw new InvalidOperationException($"Stadium with name '{stadiumDto.Name}' already exists");
				}

				var stadium = _mapper.Map<Stadium>(stadiumDto);
				await _stadiumRepository.CreateAsync(stadium);
				return _mapper.Map<StadiumResponseDto>(stadium);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating stadium");
				throw;
			}
		}

		public async Task UpdateStadiumAsync(string id, UpdateStadiumDto stadiumDto)
		{
			try
			{
				var existingStadium = await _stadiumRepository.GetByIdAsync(id);
				if (existingStadium == null)
				{
					throw new KeyNotFoundException($"Stadium with ID '{id}' not found");
				}

				var stadiumWithSameName = await _stadiumRepository.GetByNameAsync(stadiumDto.Name);
				if (stadiumWithSameName != null && stadiumWithSameName.Id != id)
				{
					throw new InvalidOperationException($"Another stadium with name '{stadiumDto.Name}' already exists");
				}

				var stadium = _mapper.Map<Stadium>(stadiumDto);
				await _stadiumRepository.UpdateAsync(id, stadium);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error updating stadium with ID: {StadiumId}", id);
				throw;
			}
		}

		public async Task DeleteStadiumAsync(string id)
		{
			try
			{
				// In a real application, check if there are events scheduled at this stadium
				await _stadiumRepository.DeleteAsync(id);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error deleting stadium with ID: {StadiumId}", id);
				throw;
			}
		}

		public async Task<StadiumSectionDetailDto> GetStadiumSectionDetailAsync(string stadiumId, string sectionId)
		{
			try
			{
				var section = await _stadiumRepository.GetSectionByIdAsync(stadiumId, sectionId);
				if (section == null)
				{
					throw new KeyNotFoundException("Section not found");
				}

				var stadium = await _stadiumRepository.GetByIdAsync(stadiumId);
				if (stadium == null)
				{
					throw new KeyNotFoundException("Stadium not found");
				}

				return new StadiumSectionDetailDto
				{
					Id = section.Id,
					Name = section.Name,
					Description = section.Description,
					StadiumId = stadiumId,
					StadiumName = stadium.Name,
					SeatRows = section.SeatRows.Select(r => new SeatRowDetailDto
					{
						Id = r.Id,
						Name = r.Name,
						Seats = r.Seats.Select(s => new SeatDetailDto
						{
							Id = s.Id,
							Number = s.Number,
							Type = s.Type,
							XPosition = s.XPosition,
							YPosition = s.YPosition
						}).ToList()
					}).ToList()
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting section detail for stadium {StadiumId}, section {SectionId}",
					stadiumId, sectionId);
				throw;
			}
		}
	}
}
