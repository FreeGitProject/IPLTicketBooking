using System.Security.Claims;
using System.Text;
using IPLTicketBooking.Models;
using IPLTicketBooking.Repositories;
using IPLTicketBooking.Services;
using IPLTicketBooking.Services.IPLTicketBooking.Services;
using IPLTicketBooking.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.Configure<MongoDBSettings>(
	builder.Configuration.GetSection("MongoDBSettings"));

builder.Services.AddSingleton<MongoDBContext>();
// Add authentication
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = builder.Configuration["Jwt:Issuer"],
		ValidAudience = builder.Configuration["Jwt:Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]))
	};
	////Debug your auth token
	//options.Events = new JwtBearerEvents
	//{
	//	OnAuthenticationFailed = context =>
	//	{
	//		Console.WriteLine($"Authentication failed: {context.Exception}");
	//		return Task.CompletedTask;
	//	},
	//	OnTokenValidated = context =>
	//	{
	//		Console.WriteLine("Token validated successfully");
	//		return Task.CompletedTask;
	//	}
	//};
});
// Add Razorpay configuration
builder.Services.Configure<RazorpaySettings>(
    builder.Configuration.GetSection("Razorpay"));
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
builder.Services.AddScoped<IUserRepository>(provider =>
	new UserRepository(provider.GetRequiredService<MongoDBContext>().Users));
builder.Services.AddScoped<IRoleRepository>(provider =>
	new RoleRepository(provider.GetRequiredService<MongoDBContext>().Roles));
// Add to your service registrations
builder.Services.AddScoped<IPaymentRepository>(provider =>
    new PaymentRepository(provider.GetRequiredService<MongoDBContext>().Payments));
// Register payment services
builder.Services.AddScoped<IRazorpayService, RazorpayService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRoleService, RoleService>();

// Register services
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<ISeatService, SeatService>();
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddAuthorization(options =>
//{
//	options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
//});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
	//Initialize sample data
	//await SeedInitialRoles(app.Services);
}
async Task SeedInitialRoles(IServiceProvider services)
{
	using var scope = services.CreateScope();
	var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();

	var roles = new[] { "Admin", "Organizer", "User" };
	foreach (var roleName in roles)
	{
		if (await roleRepository.GetByNameAsync(roleName) == null)
		{
			await roleRepository.CreateAsync(new Role
			{
				Name = roleName,
				Permissions = new List<string> { $"access:{roleName.ToLower()}" }
			});
		}
	}
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
////debug for role check
//app.Use(async (context, next) =>
//{
//	var user = context.User;
//	Console.WriteLine($"User authenticated: {user.Identity.IsAuthenticated}");
//	Console.WriteLine($"User roles: {string.Join(",", user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value))}");

//	await next();
//});
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

////Initialize sample data
//using (var scope = app.Services.CreateScope())
//{
//	var context = scope.ServiceProvider.GetRequiredService<MongoDBContext>();
//	await SampleDataInitializer.Initialize(context);
//}
app.Run();