using IPLTicketBooking.Models;
using System.Threading.Tasks;

namespace IPLTicketBooking.Repositories
{
	public interface IUserRepository
	{
		Task<User> CreateAsync(User user);
		Task<User> GetByUsernameAsync(string username);
		Task<User> GetByIdAsync(string id);
		Task UpdateAsync(User user);
	}
}