using IPLTicketBooking.Models;

namespace IPLTicketBooking.Repositories
{
	public interface IStadiumRepository
	{
		Task<IEnumerable<Stadium>> GetAllAsync();
		Task<Stadium> GetByIdAsync(string id);
		Task CreateAsync(Stadium stadium);
		Task UpdateAsync(string id, Stadium stadium);
		Task DeleteAsync(string id);
		Task<Stadium> GetByNameAsync(string name);
		Task<StadiumSection> GetSectionByIdAsync(string stadiumId, string sectionId);
	}
}
