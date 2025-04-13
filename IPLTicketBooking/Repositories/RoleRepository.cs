using IPLTicketBooking.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IPLTicketBooking.Repositories
{
	public class RoleRepository : IRoleRepository
	{
		private readonly IMongoCollection<Role> _roles;

		public RoleRepository(IMongoCollection<Role> roles)
		{
			_roles = roles;
		}

		public async Task<Role> CreateAsync(Role role)
		{
			await _roles.InsertOneAsync(role);
			return role;
		}

		public async Task<Role> GetByIdAsync(string id)
		{
			return await _roles.Find(r => r.Id == id).FirstOrDefaultAsync();
		}

		public async Task<Role> GetByNameAsync(string name)
		{
			return await _roles.Find(r => r.Name == name).FirstOrDefaultAsync();
		}

		public async Task<List<Role>> GetAllAsync()
		{
			return await _roles.Find(_ => true).ToListAsync();
		}
	}
}