namespace IPLTicketBooking.Repositories
{
	// Repositories/IMongoRepository.cs
	public interface IMongoRepository<TDocument> where TDocument : class
	{
		Task<IEnumerable<TDocument>> GetAllAsync();
		Task<TDocument> GetByIdAsync(string id);
		Task CreateAsync(TDocument document);
		Task UpdateAsync(string id, TDocument document);
		Task DeleteAsync(string id);
	}
}
