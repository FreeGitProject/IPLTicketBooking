using IPLTicketBooking.Models;
using IPLTicketBooking.Services.IPLTicketBooking.Services;
using IPLTicketBooking.Utilities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace IPLTicketBooking.Services
{
	public class SeatService : ISeatService
	{
		private readonly IMongoCollection<EventSeat> _eventSeats;
		private readonly IMongoCollection<Booking> _bookings;
		private readonly IMongoCollection<Event> _events;
		private readonly IMongoCollection<Stadium> _stadiums;
		private readonly IMongoCollection<SeatHold> _seatHolds;
		private readonly ILogger<SeatService> _logger;
		private readonly TimeSpan _seatHoldDuration = TimeSpan.FromMinutes(15);

		public SeatService(MongoDBContext context, ILogger<SeatService> logger)
		{
			_eventSeats = context.EventSeats;
			_bookings = context.Bookings;
			_events = context.Events;
			_stadiums = context.Stadiums;
			_seatHolds = context.SeatHolds;
			_logger = logger;
		}

		public async Task<IEnumerable<AvailableSeatDto>> GetAvailableSeatsAsync(string eventId)
		{
			try
			{
				// Get the event to verify it exists
				var eventObj = await _events.Find(e => e.Id == eventId).FirstOrDefaultAsync();
				if (eventObj == null)
				{
					throw new KeyNotFoundException($"Event with ID {eventId} not found");
				}

				// Get the stadium to get seat details
				var stadium = await _stadiums.Find(s => s.Id == eventObj.StadiumId).FirstOrDefaultAsync();
				if (stadium == null)
				{
					throw new KeyNotFoundException($"Stadium with ID {eventObj.StadiumId} not found");
				}

				// Get all seats for the event
				var eventSeats = await _eventSeats
					.Find(es => es.EventId == eventId)
					.ToListAsync();

				// Create a dictionary of seat details from the stadium
				var seatDetailsDict = stadium.Sections
					.SelectMany(s => s.SeatRows.SelectMany(r => r.Seats.Select(seat => new
					{
						seat.Id,
						seat.Number,
						s.Name,
						//r.Name,
						seat.Type
					})))
					.ToDictionary(x => x.Id, x => x);

				// Prepare available seats
				var availableSeats = eventSeats
					.Where(es => es.Status == "available" ||
								 (es.Status == "held" && es.HeldUntil < DateTime.UtcNow))
					.Select(es =>
					{
						seatDetailsDict.TryGetValue(es.SeatId, out var seatDetails);
						return new AvailableSeatDto
						{
							SeatId = es.Id,
							EventId = es.EventId,
							Price = es.CurrentPrice,
							Status = es.Status == "held" && es.HeldUntil < DateTime.UtcNow
									? "available"
									: es.Status,
							SeatNumber = seatDetails?.Number,
							SectionName = seatDetails?.Name,
							RowName = seatDetails?.Name,
							SeatType = seatDetails?.Type
						};
					})
					.ToList();

				return availableSeats;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving available seats for event {EventId}", eventId);
				throw;
			}
		}

		public async Task<bool> CheckSeatAvailabilityAsync(string eventId, string seatId)
		{
			try
			{
				var seat = await _eventSeats.Find(es =>
					es.EventId == eventId &&
					es.Id == seatId)
					.FirstOrDefaultAsync();

				if (seat == null) return false;

				return seat.Status == "available" ||
					  (seat.Status == "held" && seat.HeldUntil < DateTime.UtcNow);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error checking seat availability for seat {SeatId}", seatId);
				throw;
			}
		}

		public async Task<SeatHoldResult> HoldSeatsAsync(string eventId, List<string> seatIds, string userId)
		{
			try
			{
				var holdUntil = DateTime.UtcNow.Add(_seatHoldDuration);

				// 1. Verify all requested seats exist and belong to the event
				var seatsFilter = Builders<EventSeat>.Filter.In(es => es.Id, seatIds) &
								Builders<EventSeat>.Filter.Eq(es => es.EventId, eventId);

				var seats = await _eventSeats.Find(seatsFilter).ToListAsync();

				if (seats.Count != seatIds.Count)
				{
					var missingSeats = seatIds.Except(seats.Select(s => s.Id)).ToList();
					return new SeatHoldResult
					{
						Success = false,
						Message = $"Some seats not found: {string.Join(", ", missingSeats)}"
					};
				}

				// 2. Check seat availability
				var unavailableSeats = seats
					.Where(s => s.Status == "booked" ||
							  (s.Status == "held" && s.HeldUntil >= DateTime.UtcNow))
					.ToList();

				if (unavailableSeats.Any())
				{
					return new SeatHoldResult
					{
						Success = false,
						Message = $"Some seats are not available",
						UnavailableSeats = unavailableSeats.Select(s => s.Id).ToList()
					};
				}

				// 3. Hold the seats
				var holdFilter = Builders<EventSeat>.Filter.In(es => es.Id, seatIds) &
								Builders<EventSeat>.Filter.Eq(es => es.EventId, eventId) &
								Builders<EventSeat>.Filter.Or(
									Builders<EventSeat>.Filter.Eq(es => es.Status, "available"),
									Builders<EventSeat>.Filter.And(
										Builders<EventSeat>.Filter.Eq(es => es.Status, "held"),
										Builders<EventSeat>.Filter.Lt(es => es.HeldUntil, DateTime.UtcNow)
									)
								);

				var holdUpdate = Builders<EventSeat>.Update
					.Set(es => es.Status, "held")
					.Set(es => es.HeldUntil, holdUntil)
					.Inc(es => es.Version, 1);

				var updateResult = await _eventSeats.UpdateManyAsync(holdFilter, holdUpdate);

				if (updateResult.ModifiedCount != seatIds.Count)
				{
					return new SeatHoldResult
					{
						Success = false,
						Message = "Failed to hold all seats due to concurrent modification"
					};
				}

				// 4. Create a hold record
				var hold = new SeatHold
				{
					Id = ObjectId.GenerateNewId().ToString(),
					EventId = eventId,
					SeatIds = seatIds,
					UserId = userId,
					HeldUntil = holdUntil,
					CreatedAt = DateTime.UtcNow
				};

				await _seatHolds.InsertOneAsync(hold);

				return new SeatHoldResult
				{
					Success = true,
					HoldId = hold.Id,
					HeldUntil = holdUntil,
					HeldSeats = seatIds
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error holding seats for event {EventId}", eventId);
				throw;
			}
		}


		public async Task<BookingResult> BookSeatsAsync(string eventId, string holdId, List<string> seatIds, string userId)
		{
			try
			{
				// 1. Verify the seat hold is still valid
				var holdFilter = Builders<SeatHold>.Filter.Eq(h => h.Id, holdId) &
								Builders<SeatHold>.Filter.Eq(h => h.UserId, userId) &
								Builders<SeatHold>.Filter.Gt(h => h.HeldUntil, DateTime.UtcNow);

				var hold = await _seatHolds.Find(holdFilter).FirstOrDefaultAsync();

				if (hold == null)
				{
					return new BookingResult
					{
						Success = false,
						Message = "Seat hold expired or not found"
					};
				}

				// Verify the requested seats match the held seats
				if (!seatIds.All(id => hold.SeatIds.Contains(id)))
				{
					return new BookingResult
					{
						Success = false,
						Message = "Requested seats don't match held seats"
					};
				}

				// 2. Get the event for pricing and validation
				var eventObj = await _events.Find(e => e.Id == eventId).FirstOrDefaultAsync();
				if (eventObj == null)
				{
					return new BookingResult
					{
						Success = false,
						Message = "Event not found"
					};
				}

				// 3. Get the stadium for seat details
				var stadium = await _stadiums.Find(s => s.Id == eventObj.StadiumId).FirstOrDefaultAsync();
				if (stadium == null)
				{
					return new BookingResult
					{
						Success = false,
						Message = "Stadium not found"
					};
				}

				// 4. Get the current seat status
				var seatsFilter = Builders<EventSeat>.Filter.In(es => es.Id, seatIds) &
								Builders<EventSeat>.Filter.Eq(es => es.EventId, eventId) &
								Builders<EventSeat>.Filter.Eq(es => es.Status, "held") &
								Builders<EventSeat>.Filter.Gt(es => es.HeldUntil, DateTime.UtcNow);

				var heldSeats = await _eventSeats.Find(seatsFilter).ToListAsync();

				if (heldSeats.Count != seatIds.Count)
				{
					return new BookingResult
					{
						Success = false,
						Message = "Some seats are no longer held"
					};
				}

				// 5. Create seat info for the response
				var seatDetailsDict = stadium.Sections
					.SelectMany(s => s.SeatRows.SelectMany(r => r.Seats.Select(seat => new
					{
						seat.Id,
						seat.Number,
						Section = s.Name,
						Row = r.Name,
						seat.Type
					})))
					.Where(x => seatIds.Contains(x.Id))
					.ToDictionary(x => x.Id, x => x);

				var bookedSeatsInfo = heldSeats.Select(s =>
				{
					seatDetailsDict.TryGetValue(s.SeatId, out var details);
					return new BookedSeatInfo
					{
						SeatId = s.Id,
						SeatNumber = details?.Number ?? "Unknown",
						Section = details?.Section ?? "Unknown",
						Row = details?.Row ?? "Unknown",
						Price = s.CurrentPrice
					};
				}).ToList();

				// 6. Calculate total price
				var totalAmount = bookedSeatsInfo.Sum(s => s.Price);

				// 7. Create the booking
				var booking = new Booking
				{
					UserId = userId,
					EventId = eventId,
					Seats = bookedSeatsInfo.Select(s => new BookedSeat
					{
						SeatId = s.SeatId,
						Price = s.Price
					}).ToList(),
					TotalAmount = totalAmount,
					Status = "confirmed",
					PaymentId = null,
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				};

				await _bookings.InsertOneAsync(booking);

				// 8. Update seat status to booked
				var seatUpdate = Builders<EventSeat>.Update
					.Set(es => es.Status, "booked")
					.Set(es => es.HeldUntil, null)
					.Inc(es => es.Version, 1);

				await _eventSeats.UpdateManyAsync(
					Builders<EventSeat>.Filter.In(es => es.Id, seatIds),
					seatUpdate);

				// 9. Remove the hold record
				await _seatHolds.DeleteOneAsync(holdFilter);

				return new BookingResult
				{
					Success = true,
					BookingId = booking.Id,
					TotalAmount = totalAmount,
					BookingDate = booking.CreatedAt,
					BookedSeats = bookedSeatsInfo
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error booking seats for event {EventId}", eventId);
				throw;
			}
		}


		public async Task<int> ReleaseExpiredHoldsAsync()
		{
			try
			{
				// Release seats with expired holds
				var seatFilter = Builders<EventSeat>.Filter.Eq(es => es.Status, "held") &
								Builders<EventSeat>.Filter.Lt(es => es.HeldUntil, DateTime.UtcNow);

				var seatUpdate = Builders<EventSeat>.Update
					.Set(es => es.Status, "available")
					.Set(es => es.HeldUntil, null)
					.Inc(es => es.Version, 1);

				var seatResult = await _eventSeats.UpdateManyAsync(seatFilter, seatUpdate);

				// Delete expired hold records
				var holdFilter = Builders<SeatHold>.Filter.Lt(h => h.HeldUntil, DateTime.UtcNow);
				await _seatHolds.DeleteManyAsync(holdFilter);

				return (int)seatResult.ModifiedCount;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error releasing expired holds");
				throw;
			}
		}

		public async Task<Booking> GetBookingByIdAsync(string bookingId)
		{
			try
			{
				return await _bookings.Find(b => b.Id == bookingId).FirstOrDefaultAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting booking by ID {BookingId}", bookingId);
				throw;
			}
		}

		public async Task<IEnumerable<Booking>> GetUserBookingsAsync(string userId)
		{
			try
			{
				return await _bookings.Find(b => b.UserId == userId)
					.SortByDescending(b => b.CreatedAt)
					.ToListAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting bookings for user {UserId}", userId);
				throw;
			}
		}
	}
}