using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var assemblyName = typeof(Program).Assembly.GetName().Name;
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<StoreContext>(opt =>
{
    //opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    opt.UseNpgsql(connectionString, m => m.MigrationsAssembly(assemblyName));
});

builder.Services.AddIdentityApiEndpoints<User>(opt =>
{
    opt.User.RequireUniqueEmail = true;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<StoreContext>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();


app.UseCors(opt =>
{
    opt.AllowAnyHeader()
       .AllowAnyMethod()
       .AllowCredentials()
       .WithOrigins("http://localhost:5173", "https://localhost:5173");
});


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// A more robust way to ensure migrations are applied and the database is seeded.
// This prevents a race condition where the application starts before the database is ready.
try
{
    // Create a service scope to get a fresh instance of the database context and other services.
    var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<StoreContext>();

    // Asynchronously apply any pending database migrations.
    await context.Database.MigrateAsync();

    // Now that the database schema is up-to-date, seed the database.
    // We pass the entire 'app' instance, as your DbInitializer is configured to use it.
    await DbInitializer.InitDb(app);
}
catch (Exception ex)
{
    // If an error occurs during migration or seeding, log it.
    // This prevents the application from crashing and provides helpful debugging info.
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during database migration or seeding.");
}

app.MapGroup("api").MapIdentityApi<User>(); //api/login

app.Run();
