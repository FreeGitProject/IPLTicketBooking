using IPLTicketBooking.Models;
using MongoDB.Driver;

namespace IPLTicketBooking.Repositories
{
	public class StadiumRepository : IStadiumRepository
	{
		private readonly IMongoCollection<Stadium> _stadiums;

		public StadiumRepository(IMongoCollection<Stadium> stadiums)
		{
			_stadiums = stadiums;
		}

		public async Task<IEnumerable<Stadium>> GetAllAsync()
		{
			return await _stadiums.Find(_ => true).ToListAsync();
		}

		public async Task<Stadium> GetByIdAsync(string id)
		{
			return await _stadiums.Find(s => s.Id == id).FirstOrDefaultAsync();
		}

		public async Task<Stadium> GetByNameAsync(string name)
		{
			return await _stadiums.Find(s => s.Name == name).FirstOrDefaultAsync();
		}

		public async Task CreateAsync(Stadium stadium)
		{
			await _stadiums.InsertOneAsync(stadium);
		}

		public async Task UpdateAsync(string id, Stadium stadium)
		{
			await _stadiums.ReplaceOneAsync(s => s.Id == id, stadium);
		}

		public async Task DeleteAsync(string id)
		{
			await _stadiums.DeleteOneAsync(s => s.Id == id);
		}
		public async Task<StadiumSection> GetSectionByIdAsync(string stadiumId, string sectionId)
		{
			var stadium = await _stadiums.Find(s => s.Id == stadiumId).FirstOrDefaultAsync();
			if (stadium == null) return null;

			return stadium.Sections.FirstOrDefault(s => s.Id == sectionId);
		}
	}
}
