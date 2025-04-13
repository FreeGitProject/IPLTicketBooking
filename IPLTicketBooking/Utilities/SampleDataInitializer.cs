using IPLTicketBooking.Models;
using MongoDB.Driver;

namespace IPLTicketBooking.Utilities
{
	// Utilities/SampleDataInitializer.cs
	public static class SampleDataInitializer
	{
		public static async Task Initialize(MongoDBContext context)
		{
			// Check if data already exists
			var hasCategories = await context.Categories.Find(_ => true).AnyAsync();
			if (hasCategories) return;

			// Create Cricket category
			var cricketCategory = new Category
			{
				Name = "Cricket",
				Description = "All cricket related events including IPL matches"
			};
			await context.Categories.InsertOneAsync(cricketCategory);

			// Create Wankhede Stadium
			var wankhede = new Stadium
			{
				Name = "Wankhede Stadium",
				Location = "Mumbai, India",
				Capacity = 33000,
				Sections = new List<StadiumSection>
			{
				new StadiumSection
				{
					Name = "North Stand",
					Description = "Lower tier seating",
					SeatRows = GenerateSeatRows(20, 50, "N", "standard")
				},
				new StadiumSection
				{
					Name = "Pavilion",
					Description = "Premium seating area",
					SeatRows = GenerateSeatRows(10, 30, "P", "premium")
				},
				new StadiumSection
				{
					Name = "VIP Box",
					Description = "Exclusive VIP seating",
					SeatRows = GenerateSeatRows(5, 10, "V", "vip")
				}
			}
			};
			await context.Stadiums.InsertOneAsync(wankhede);

			// Create an IPL match event
			var iplMatch = new Event
			{
				Name = "IPL 2024: Mumbai Indians vs Chennai Super Kings",
				CategoryId = cricketCategory.Id,
				StadiumId = wankhede.Id,
				DateTime = DateTime.UtcNow.AddDays(30),
				Duration = 240, // 4 hours
				Description = "Blockbuster IPL match between MI and CSK",
				BasePrice = 1500,
				Status = "upcoming"
			};
			await context.Events.InsertOneAsync(iplMatch);

			// Generate event seats
			var allSeats = wankhede.Sections
				.SelectMany(s => s.SeatRows.SelectMany(r => r.Seats))
				.ToList();

			var eventSeats = allSeats.Select(seat => new EventSeat
			{
				EventId = iplMatch.Id,
				SeatId = seat.Id,
				Status = "available",
				CurrentPrice = CalculateSeatPrice(seat.Type, iplMatch.BasePrice)
			}).ToList();

			await context.EventSeats.InsertManyAsync(eventSeats);
		}

		private static List<SeatRow> GenerateSeatRows(int rowCount, int seatsPerRow, string prefix, string type)
		{
			var rows = new List<SeatRow>();

			for (int i = 1; i <= rowCount; i++)
			{
				var row = new SeatRow
				{
					Name = $"{prefix}{i}",
					Seats = new List<Seat>()
				};

				for (int j = 1; j <= seatsPerRow; j++)
				{
					row.Seats.Add(new Seat
					{
						Number = $"{i}-{j}",
						Type = type,
						XPosition = j,
						YPosition = i
					});
				}

				rows.Add(row);
			}

			return rows;
		}

		private static decimal CalculateSeatPrice(string type, decimal basePrice)
		{
			return type switch
			{
				"vip" => basePrice * 3,
				"premium" => basePrice * 2,
				_ => basePrice
			};
		}
	}
}
