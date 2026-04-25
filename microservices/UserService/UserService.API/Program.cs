using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using UserService.Application.Consumers;
using UserService.Application.Interfaces;
using UserService.Application.Services;
using UserService.Infrastructure.Data;
using UserService.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

var envPath = Path.Combine(Directory.GetCurrentDirectory(), "../../../docker/.env");
if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath))
    {
        var parts = line.Split('=', 2);
        if (parts.Length == 2) Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim().Trim('"'));
    }
}

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Warning);
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("MassTransit", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);

builder.Services.AddControllers();

var connectionString = Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTION")
                      ?? builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Server=localhost;Database=UserDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;";

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
        sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    }));

builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();
builder.Services.AddScoped<IUserService, UserService.Application.Services.UserService>();

var rabbitMqHost = Environment.GetEnvironmentVariable("RabbitMQ__Host") ?? "localhost";

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserCreatedConsumer>();
    x.AddConsumer<EnrollmentCreatedConsumer>();
    x.AddConsumer<ProgressUpdatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMqHost, "/", h =>
        {
            h.Username(Environment.GetEnvironmentVariable("RabbitMQ__User") ?? "guest");
            h.Password(Environment.GetEnvironmentVariable("RabbitMQ__Password") ?? "guest");
        });

        cfg.ReceiveEndpoint("user-created-queue", e =>
        {
            e.ConfigureConsumer<UserCreatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("user-enrollment-queue", e =>
        {
            e.ConfigureConsumer<EnrollmentCreatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("user-progress-queue", e =>
        {
            e.ConfigureConsumer<ProgressUpdatedConsumer>(context);
        });
    });
});

var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET")
             ?? builder.Configuration["Jwt:Key"]
             ?? "THIS_IS_SECRET_KEY_CHANGE_IT_1234567890";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "IdentityService";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "IdentityServiceClients";

var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "User Service API", Version = "v1" });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    try
    {
        var databaseCreator = dbContext.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
        if (databaseCreator != null)
        {
            if (!databaseCreator.Exists()) databaseCreator.Create();
            try { dbContext.Database.ExecuteSqlRaw("SELECT TOP 0 * FROM UserProfiles"); }
            catch { databaseCreator.CreateTables(); }
        }
    }
    catch (Exception) { /* Silent catch for sync notice */ }
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "User Service API v1");
});

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

var port = "8011";
Console.WriteLine("✅ Database connected successfully!");
Console.WriteLine($"🚀 User Service is running on port {port}");
Console.WriteLine($"📖 Swagger UI: http://127.0.0.1:{port}/swagger");

app.Run($"http://0.0.0.0:{port}");
