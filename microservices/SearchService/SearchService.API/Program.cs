using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SearchService.Application.Interfaces;
using SearchService.Infrastructure.Services;
using MassTransit;
using SearchService.Application.Consumers;

// --- Custom .env Loader ---
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

// MassTransit Configuration
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<CourseApprovedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost", "/", h =>
        {
            h.Username(Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest");
            h.Password(Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest");
        });

        cfg.ReceiveEndpoint("search-course-approved", e =>
        {
            e.ConfigureConsumer<CourseApprovedConsumer>(context);
        });
    });
});

// --- Clean Logging Configuration ---
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);

builder.Services.AddControllers();

// Dependency Injection
builder.Services.AddScoped<ISearchService, MeiliSearchService>();

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
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Search Service API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter JWT token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme {
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, new string[] { }
    } });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "Search Service API v1"); options.RoutePrefix = "swagger"; });

app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapControllers();

var port = "8014";
Console.WriteLine($"🚀 Search Service is running on port {port}");
Console.WriteLine($"📖 Swagger UI: http://localhost:{port}/swagger");

app.Run($"http://0.0.0.0:{port}");
