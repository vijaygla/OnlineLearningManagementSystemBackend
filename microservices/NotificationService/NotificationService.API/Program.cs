using MassTransit;
using NotificationService.Application.Configurations;
using NotificationService.Application.Consumers;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Services;

// --- Custom .env Loader (following the project pattern) ---
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

// Clean logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Warning);
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("MassTransit", LogLevel.Error);
builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);

// Email Configuration
builder.Services.Configure<EmailSettings>(options =>
{
    options.Host = Environment.GetEnvironmentVariable("Smtp__Host") ?? builder.Configuration["Smtp:Host"] ?? "localhost";
    options.Port = int.Parse(Environment.GetEnvironmentVariable("Smtp__Port") ?? builder.Configuration["Smtp:Port"] ?? "1025");
    options.SenderEmail = Environment.GetEnvironmentVariable("Smtp__SenderEmail") ?? builder.Configuration["Smtp:SenderEmail"] ?? "noreply@lms.com";
    options.SenderName = Environment.GetEnvironmentVariable("Smtp__SenderName") ?? builder.Configuration["Smtp:SenderName"] ?? "LMS Notifications";
    options.Password = Environment.GetEnvironmentVariable("Smtp__Password") ?? builder.Configuration["Smtp:Password"] ?? "";
});

builder.Services.AddScoped<IEmailService, EmailService>();

// MassTransit Configuration
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<EnrollmentCreatedConsumer>();

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
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapGet("/", () => "Notification Service is running");
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

var port = "8010";
Console.WriteLine($"🚀 Notification Service is running on port {port}");
Console.WriteLine($"📖 Swagger UI: http://localhost:{port}/swagger");

app.Run($"http://0.0.0.0:{port}");
