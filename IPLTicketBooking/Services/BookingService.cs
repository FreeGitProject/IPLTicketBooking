using IPLTicketBooking.Models;
using IPLTicketBooking.Services.IPLTicketBooking.Services;
using IPLTicketBooking.Utilities;
using MongoDB.Driver;

namespace IPLTicketBooking.Services
{
    public class BookingService : IBookingService
    {
        private readonly IMongoCollection<Booking> _bookings;
        private readonly IMongoCollection<EventSeat> _eventSeats;
        private readonly IMongoCollection<Event> _events;
        private readonly IMongoCollection<Payment> _payments;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            MongoDBContext context,
            ILogger<BookingService> logger)
        {
            _bookings = context.Bookings;
            _eventSeats = context.EventSeats;
            _events = context.Events;
            _payments = context.Payments;
            _logger = logger;
        }

        public async Task<BookingResult> BookSeatsAsync(
            string eventId,
            string holdId,
            List<string> seatIds,
            string userId)
        {
            using var session = await _bookings.Database.Client.StartSessionAsync();
            session.StartTransaction();

            try
            {
                // 1. Verify the seat hold is still valid
                var heldSeatsFilter = Builders<EventSeat>.Filter.In(es => es.Id, seatIds) &
                                    Builders<EventSeat>.Filter.Eq(es => es.EventId, eventId) &
                                    Builders<EventSeat>.Filter.Eq(es => es.Status, "held");

                var heldSeats = await _eventSeats.Find(session, heldSeatsFilter).ToListAsync();

                if (heldSeats.Count != seatIds.Count)
                {
                    await session.AbortTransactionAsync();
                    return new BookingResult
                    {
                        Success = false,
                        Message = "Some seats are no longer held"
                    };
                }

                // 2. Get event details for pricing
                var eventObj = await _events.Find(session, e => e.Id == eventId).FirstOrDefaultAsync();
                if (eventObj == null)
                {
                    await session.AbortTransactionAsync();
                    return new BookingResult
                    {
                        Success = false,
                        Message = "Event not found"
                    };
                }

                // 3. Calculate total amount
                var totalAmount = heldSeats.Sum(s => s.CurrentPrice);

                // 4. Create the booking
                var booking = new Booking
                {
                    UserId = userId,
                    EventId = eventId,
                    Seats = heldSeats.Select(s => new BookedSeat
                    {
                        SeatId = s.Id,
                        Price = s.CurrentPrice
                    }).ToList(),
                    TotalAmount = totalAmount,
                    Status = "pending_payment", // Will change to confirmed after payment
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _bookings.InsertOneAsync(session, booking);

                // 5. Update seat status to booked
                var seatUpdate = Builders<EventSeat>.Update
                    .Set(es => es.Status, "booked")
                    .Set(es => es.HeldUntil, null)
                    .Inc(es => es.Version, 1);

                await _eventSeats.UpdateManyAsync(
                    session,
                    Builders<EventSeat>.Filter.In(es => es.Id, seatIds),
                    seatUpdate);

                await session.CommitTransactionAsync();

                return new BookingResult
                {
                    Success = true,
                    BookingId = booking.Id,
                    TotalAmount = totalAmount
                };
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                _logger.LogError(ex, "Error booking seats for event {EventId}", eventId);
                throw;
            }
        }

        public async Task<BookingResult> ConfirmBookingWithPayment(
            string bookingId,
            string paymentId)
        {
            using var session = await _bookings.Database.Client.StartSessionAsync();
            session.StartTransaction();

            try
            {
                // 1. Update booking status and payment ID
                var bookingFilter = Builders<Booking>.Filter.Eq(b => b.Id, bookingId);
                var bookingUpdate = Builders<Booking>.Update
                    .Set(b => b.PaymentId, paymentId)
                    .Set(b => b.Status, "confirmed")
                    .Set(b => b.UpdatedAt, DateTime.UtcNow);

                var bookingResult = await _bookings.UpdateOneAsync(
                    session,
                    bookingFilter,
                    bookingUpdate);

                if (bookingResult.ModifiedCount == 0)
                {
                    await session.AbortTransactionAsync();
                    return new BookingResult
                    {
                        Success = false,
                        Message = "Booking not found"
                    };
                }

                // 2. Create payment record
                var payment = new Payment
                {
                    BookingId = bookingId,
                    RazorpayPaymentId = paymentId,
                    Status = "captured",
                    Amount = (await _bookings.Find(session, bookingFilter).FirstOrDefaultAsync())?.TotalAmount ?? 0,
                    Currency = "INR",
                    CreatedAt = DateTime.UtcNow
                };

                await _payments.InsertOneAsync(session, payment);

                await session.CommitTransactionAsync();

                return new BookingResult
                {
                    Success = true,
                    BookingId = bookingId
                };
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                _logger.LogError(ex, "Error confirming booking payment {BookingId}", bookingId);
                throw;
            }
        }

        public async Task<Booking> GetBookingByIdAsync(string id)
        {
            try
            {
                var booking = await _bookings.Find(b => b.Id == id).FirstOrDefaultAsync();
                if (booking != null)
                {
                    // Populate event details if needed
                    booking.Event = await _events.Find(e => e.Id == booking.EventId).FirstOrDefaultAsync();
                }
                return booking;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking {BookingId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Booking>> GetUserBookingsAsync(string userId)
        {
            try
            {
                var bookings = await _bookings.Find(b => b.UserId == userId)
                    .SortByDescending(b => b.CreatedAt)
                    .ToListAsync();

                // Populate event details for each booking
                var eventIds = bookings.Select(b => b.EventId).Distinct();
                var events = await _events.Find(e => eventIds.Contains(e.Id)).ToListAsync();

                foreach (var booking in bookings)
                {
                    booking.Event = events.FirstOrDefault(e => e.Id == booking.EventId);
                }

                return bookings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> CancelBookingAsync(string bookingId, string userId, bool isAdmin = false)
        {
            using var session = await _bookings.Database.Client.StartSessionAsync();
            session.StartTransaction();

            try
            {
                // Build filter based on user/admin status
                var bookingFilter = Builders<Booking>.Filter.Eq(b => b.Id, bookingId);

                if (!isAdmin)
                {
                    bookingFilter &= Builders<Booking>.Filter.Eq(b => b.UserId, userId);
                }

                bookingFilter &= Builders<Booking>.Filter.In(b => b.Status,
                    new[] { "pending_payment", "confirmed" });

                var booking = await _bookings.Find(session, bookingFilter).FirstOrDefaultAsync();
                if (booking == null) return false;

                // 1. Update booking status
                var bookingUpdate = Builders<Booking>.Update
                    .Set(b => b.Status, "cancelled")
                    .Set(b => b.UpdatedAt, DateTime.UtcNow);

                await _bookings.UpdateOneAsync(session, bookingFilter, bookingUpdate);

                // 2. Release seats
                var seatIds = booking.Seats.Select(s => s.SeatId).ToList();
                var seatUpdate = Builders<EventSeat>.Update
                    .Set(es => es.Status, "available")
                    .Set(es => es.HeldUntil, null);

                await _eventSeats.UpdateManyAsync(
                    session,
                    Builders<EventSeat>.Filter.In(es => es.Id, seatIds),
                    seatUpdate);

                // 3. Process refund if payment was made
                if (!string.IsNullOrEmpty(booking.PaymentId))
                {
                    // In production, you would:
                    // 1. Call Razorpay refund API
                    // 2. Create refund record
                    // 3. Update payment status

                    var paymentUpdate = Builders<Payment>.Update
                        .Set(p => p.Status, "refund_initiated");

                    await _payments.UpdateOneAsync(
                        session,
                        Builders<Payment>.Filter.Eq(p => p.RazorpayPaymentId, booking.PaymentId),
                        paymentUpdate);

                    // Note: Actual refund processing would be handled by a background service
                }

                await session.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                _logger.LogError(ex, "Error cancelling booking {BookingId}", bookingId);
                throw;
            }
        }

        //public async Task<bool> CancelBookingAsync(string bookingId, string userId)
        //{
        //    using var session = await _bookings.Database.Client.StartSessionAsync();
        //    session.StartTransaction();

        //    try
        //    {
        //        // 1. Verify booking exists and belongs to user
        //        var bookingFilter = Builders<Booking>.Filter.Eq(b => b.Id, bookingId) &
        //                          Builders<Booking>.Filter.Eq(b => b.UserId, userId) &
        //                          Builders<Booking>.Filter.In(b => b.Status,
        //                              new[] { "pending_payment", "confirmed" });

        //        var booking = await _bookings.Find(session, bookingFilter).FirstOrDefaultAsync();
        //        if (booking == null)
        //        {
        //            await session.AbortTransactionAsync();
        //            return false;
        //        }

        //        // 2. Update booking status
        //        var bookingUpdate = Builders<Booking>.Update
        //            .Set(b => b.Status, "cancelled")
        //            .Set(b => b.UpdatedAt, DateTime.UtcNow);

        //        await _bookings.UpdateOneAsync(
        //            session,
        //            bookingFilter,
        //            bookingUpdate);

        //        // 3. Release seats back to available
        //        var seatIds = booking.Seats.Select(s => s.SeatId).ToList();
        //        var seatUpdate = Builders<EventSeat>.Update
        //            .Set(es => es.Status, "available")
        //            .Set(es => es.HeldUntil, null);

        //        await _eventSeats.UpdateManyAsync(
        //            session,
        //            Builders<EventSeat>.Filter.In(es => es.Id, seatIds),
        //            seatUpdate);

        //        // 4. Initiate refund if payment was made
        //        if (!string.IsNullOrEmpty(booking.PaymentId))
        //        {
        //            // In a real implementation, you would call Razorpay's refund API here
        //            var paymentUpdate = Builders<Payment>.Update
        //                .Set(p => p.Status, "refunded");

        //            await _payments.UpdateOneAsync(
        //                session,
        //                Builders<Payment>.Filter.Eq(p => p.RazorpayPaymentId, booking.PaymentId),
        //                paymentUpdate);
        //        }

        //        await session.CommitTransactionAsync();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        await session.AbortTransactionAsync();
        //        _logger.LogError(ex, "Error cancelling booking {BookingId}", bookingId);
        //        throw;
        //    }
        //}
    }
}