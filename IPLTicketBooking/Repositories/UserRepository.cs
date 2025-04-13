using IPLTicketBooking.Models;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace IPLTicketBooking.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly IMongoCollection<User> _users;

		public UserRepository(IMongoCollection<User> users)
		{
			_users = users;
		}

		public async Task<User> CreateAsync(User user)
		{
			await _users.InsertOneAsync(user);
			return user;
		}

		public async Task<User> GetByUsernameAsync(string username)
		{
			return await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
		}

		public async Task<User> GetByIdAsync(string id)
		{
			return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
		}

		public async Task UpdateAsync(User user)
		{
			await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
		}
	}
}