using IPLTicketBooking.Models;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace IPLTicketBooking.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly IMongoCollection<Payment> _payments;

        public PaymentRepository(IMongoCollection<Payment> payments)
        {
            _payments = payments;
        }

        public async Task<Payment> GetByIdAsync(string id)
        {
            return await _payments.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Payment> GetByRazorpayPaymentIdAsync(string razorpayPaymentId)
        {
            return await _payments.Find(p => p.RazorpayPaymentId == razorpayPaymentId).FirstOrDefaultAsync();
        }

        public async Task<Payment> CreateAsync(Payment payment)
        {
            await _payments.InsertOneAsync(payment);
            return payment;
        }

        public async Task UpdateAsync(string id, Payment payment)
        {
            await _payments.ReplaceOneAsync(p => p.Id == id, payment);
        }

        public async Task DeleteAsync(string id)
        {
            await _payments.DeleteOneAsync(p => p.Id == id);
        }
    }
}