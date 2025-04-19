using IPLTicketBooking.Models;

namespace IPLTicketBooking.Repositories
{
    public interface IPaymentRepository
    {
        Task<Payment> GetByIdAsync(string id);
        Task<Payment> GetByRazorpayPaymentIdAsync(string razorpayPaymentId);
        Task<Payment> CreateAsync(Payment payment);
        Task UpdateAsync(string id, Payment payment);
        Task DeleteAsync(string id);
    }
}