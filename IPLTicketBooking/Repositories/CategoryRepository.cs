using IPLTicketBooking.Models;
using MongoDB.Driver;

namespace IPLTicketBooking.Repositories
{
	// Repositories/CategoryRepository.cs
	public class CategoryRepository : MongoRepository<Category>, ICategoryRepository
	{
		public CategoryRepository(IMongoCollection<Category> collection) : base(collection)
		{
		}

		public async Task<Category> GetByNameAsync(string name)
		{
			var filter = Builders<Category>.Filter.Eq(c => c.Name, name);
			return await _collection.Find(filter).FirstOrDefaultAsync();
		}
	}
}
