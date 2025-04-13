using IPLTicketBooking.Models;

namespace IPLTicketBooking.Repositories
{
	// Repositories/ICategoryRepository.cs
	public interface ICategoryRepository : IMongoRepository<Category>
	{
		Task<Category> GetByNameAsync(string name);
	}
}
