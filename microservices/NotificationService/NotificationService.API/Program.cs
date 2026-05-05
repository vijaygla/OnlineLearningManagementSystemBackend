using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NotificationService.Application.Configurations;
using NotificationService.Application.Consumers;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Services;
using NotificationService.Infrastructure.Data;
using NotificationService.Infrastructure.Services;
using SharedKernel.Utilities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

// --- Custom .env Loader ---
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "../../../docker/.env");
if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath))
    {
        if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#')) continue;
        var parts = line.Split('=', 2);
        if (parts.Length == 2) Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim().Trim('"'));
    }
}

var builder = WebApplication.CreateBuilder(args);

// Clean logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Warning);
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("MassTransit", LogLevel.Error);
builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);

builder.Services.AddControllers();

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Database Configuration
var connectionString = Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTION") ?? builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
    }));

// Email Configuration
builder.Services.Configure<EmailSettings>(options =>
{
    options.Host = Environment.GetEnvironmentVariable("SMTP_HOST") ?? builder.Configuration["Smtp:Host"] ?? "localhost";
    options.Port = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? builder.Configuration["Smtp:Port"] ?? "1025");
    options.Username = Environment.GetEnvironmentVariable("SMTP_USER") ?? builder.Configuration["Smtp:Username"] ?? "";
    options.SenderEmail = Environment.GetEnvironmentVariable("SMTP_SENDER_EMAIL") ?? builder.Configuration["Smtp:SenderEmail"] ?? "noreply@lms.com";
    options.SenderName = Environment.GetEnvironmentVariable("SMTP_SENDER_NAME") ?? builder.Configuration["Smtp:SenderName"] ?? "LMS Notifications";
    options.Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? builder.Configuration["Smtp:Password"] ?? "";
});

// Dependency Injection
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<INotificationService, InAppNotificationService>();

// MassTransit Configuration
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<EnrollmentCreatedConsumer>();
    x.AddConsumer<UserCreatedConsumer>();
    x.AddConsumer<ForgotPasswordConsumer>();
    x.AddConsumer<CourseApprovedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = Environment.GetEnvironmentVariable("RabbitMQ__Host") ?? "localhost";
        var rabbitUser = Environment.GetEnvironmentVariable("RabbitMQ__User") ?? "guest";
        var rabbitPass = Environment.GetEnvironmentVariable("RabbitMQ__Password") ?? "guest";

        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(rabbitUser);
            h.Password(rabbitPass);
        });

        cfg.ReceiveEndpoint("enrollment-created-queue", e =>
        {
            e.ConfigureConsumer<EnrollmentCreatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("user-created-queue", e =>
        {
            e.ConfigureConsumer<UserCreatedConsumer>(context);
        });

        cfg.ReceiveEndpoint("forgot-password-queue", e =>
        {
            e.ConfigureConsumer<ForgotPasswordConsumer>(context);
        });

        cfg.ReceiveEndpoint("course-approved-queue", e =>
        {
            e.ConfigureConsumer<CourseApprovedConsumer>(context);
        });
    });
});

// Authentication
var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET")
             ?? builder.Configuration["Jwt:Key"]
             ?? "THIS_IS_SECRET_KEY_CHANGE_IT_1234567890";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "IdentityService";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "IdentityServiceClients";

var key = Encoding.UTF8.GetBytes(jwtKey);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
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
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Notification Service API", Version = "v1" });
});

var app = builder.Build();

// Ensure Database and Tables exist
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    try
    {
        var databaseCreator = dbContext.GetService<IRelationalDatabaseCreator>();
        if (!databaseCreator.Exists()) databaseCreator.Create();
        
        // Use a more robust check for shared database: try to create tables and ignore if they already exist
        try 
        { 
            databaseCreator.CreateTables(); 
        } 
        catch 
        { 
            // Tables likely already exist, which is fine in a shared database environment
        }
        
        Console.WriteLine("✅ Database connected successfully!");
    }
    catch
    {
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

var port = "8010";
PortReclaimer.Reclaim(int.Parse(port));

Console.WriteLine($"🚀 Notification Service is running on port {port}");
Console.WriteLine($"📖 Swagger UI: http://localhost:{port}/swagger");

app.Run($"http://0.0.0.0:{port}");
