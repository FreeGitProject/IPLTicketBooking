using AutoMapper;
using IPLTicketBooking.DTOs;
using IPLTicketBooking.Models;

namespace IPLTicketBooking.Utilities
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			// Stadium mappings
			CreateMap<CreateStadiumDto, Stadium>();
			CreateMap<UpdateStadiumDto, Stadium>();
			CreateMap<Stadium, StadiumResponseDto>();
			CreateMap<StadiumSectionDto, StadiumSection>();
			CreateMap<SeatRowDto, SeatRow>();
			CreateMap<SeatDto, Seat>();
			CreateMap<StadiumSection, StadiumSectionResponseDto>();
			CreateMap<StadiumSection, StadiumSectionDetailDto>();
			CreateMap<SeatRow, SeatRowDetailDto>();
			CreateMap<Seat, SeatDetailDto>();
			CreateMap<Category, CategoryResponseDto>();
			CreateMap<Category, CreateCategoryDto>().ReverseMap();
			CreateMap< UpdateCategoryDto, Category>();
		}
	}
}
