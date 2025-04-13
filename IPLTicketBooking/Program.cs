using IPLTicketBooking.Models;
using IPLTicketBooking.Repositories;
using IPLTicketBooking.Services;
using IPLTicketBooking.Services.IPLTicketBooking.Services;
using IPLTicketBooking.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.Configure<MongoDBSettings>(
	builder.Configuration.GetSection("MongoDBSettings"));

builder.Services.AddSingleton<MongoDBContext>();

// Register repositories
builder.Services.AddScoped<IMongoRepository<Category>>(provider =>
	new MongoRepository<Category>(provider.GetRequiredService<MongoDBContext>().Categories));
builder.Services.AddScoped<IEventRepository>(provider =>
	new EventRepository(provider.GetRequiredService<MongoDBContext>().Events));
builder.Services.AddScoped<ICategoryRepository>(provider =>
	new CategoryRepository(provider.GetRequiredService<MongoDBContext>().Categories));
// Register Stadium repository and service
builder.Services.AddScoped<IStadiumRepository>(provider =>
	new StadiumRepository(provider.GetRequiredService<MongoDBContext>().Stadiums));
builder.Services.AddScoped<IStadiumService, StadiumService>();

// Register services
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<ISeatService, SeatService>();
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

//// Background service to release expired holds
//var seatService = app.Services.GetRequiredService<ISeatService>();
//var timer = new Timer(async _ =>
//{
//	try
//	{
//		await seatService.ReleaseExpiredHoldsAsync();
//	}
//	catch (Exception ex)
//	{
//		var logger = app.Services.GetRequiredService<ILogger<Program>>();
//		logger.LogError(ex, "Error releasing expired holds");
//	}
//}, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

// Initialize sample data
//using (var scope = app.Services.CreateScope())
//{
//	var context = scope.ServiceProvider.GetRequiredService<MongoDBContext>();
//	await SampleDataInitializer.Initialize(context);
//}
app.Run();