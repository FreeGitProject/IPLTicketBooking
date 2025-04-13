using MongoDB.Bson;
using MongoDB.Driver;

namespace IPLTicketBooking.Repositories
{
	// Repositories/MongoRepository.cs
	public class MongoRepository<TDocument> : IMongoRepository<TDocument> where TDocument : class
	{
		public readonly IMongoCollection<TDocument> _collection;

		public MongoRepository(IMongoCollection<TDocument> collection)
		{
			_collection = collection;
		}

		public async Task<IEnumerable<TDocument>> GetAllAsync()
		{
			return await _collection.Find(_ => true).ToListAsync();
		}

		public async Task<TDocument> GetByIdAsync(string id)
		{
			var filter = Builders<TDocument>.Filter.Eq("_id", ObjectId.Parse(id));
			return await _collection.Find(filter).FirstOrDefaultAsync();
		}

		public async Task CreateAsync(TDocument document)
		{
			await _collection.InsertOneAsync(document);
		}

		public async Task UpdateAsync(string id, TDocument document)
		{
			var filter = Builders<TDocument>.Filter.Eq("_id", ObjectId.Parse(id));
			await _collection.ReplaceOneAsync(filter, document);
		}

		public async Task DeleteAsync(string id)
		{
			var filter = Builders<TDocument>.Filter.Eq("_id", ObjectId.Parse(id));
			await _collection.DeleteOneAsync(filter);
		}
	}

}
