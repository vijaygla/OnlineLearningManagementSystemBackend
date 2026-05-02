using MassTransit;
using NotificationService.Application.Configurations;
using NotificationService.Application.Consumers;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Services;

// --- Custom .env Loader ---
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "../../../docker/.env");
if (File.Exists(envPath))
{
    int count = 0;
    foreach (var line in File.ReadAllLines(envPath))
    {
        if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#')) continue;
        
        var parts = line.Split('=', 2);
        if (parts.Length == 2)
        {
            Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim().Trim('"'));
            count++;
        }
    }
    Console.WriteLine($"⚙️ Loaded {count} variables from {envPath}");
}
else
{
    Console.WriteLine($"⚠️ .env file NOT found at: {envPath}");
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
    options.Host = Environment.GetEnvironmentVariable("SMTP_HOST") ?? builder.Configuration["Smtp:Host"] ?? "localhost";
    options.Port = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? builder.Configuration["Smtp:Port"] ?? "1025");
    options.Username = Environment.GetEnvironmentVariable("SMTP_USER") ?? builder.Configuration["Smtp:Username"] ?? "";
    options.SenderEmail = Environment.GetEnvironmentVariable("SMTP_SENDER_EMAIL") ?? builder.Configuration["Smtp:SenderEmail"] ?? "noreply@lms.com";
    options.SenderName = Environment.GetEnvironmentVariable("SMTP_SENDER_NAME") ?? builder.Configuration["Smtp:SenderName"] ?? "LMS Notifications";
    options.Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? builder.Configuration["Smtp:Password"] ?? "";
});

builder.Services.AddScoped<IEmailService, EmailService>();

// MassTransit Configuration
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<EnrollmentCreatedConsumer>();
    x.AddConsumer<UserCreatedConsumer>();
    x.AddConsumer<ForgotPasswordConsumer>();

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
