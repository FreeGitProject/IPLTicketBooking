using IPLTicketBooking.Models;

namespace IPLTicketBooking.Repositories
{
	public interface IRoleRepository
	{
		Task<Role> CreateAsync(Role role);
		Task<Role> GetByIdAsync(string id);
		Task<Role> GetByNameAsync(string name);
		Task<List<Role>> GetAllAsync();
	}
}