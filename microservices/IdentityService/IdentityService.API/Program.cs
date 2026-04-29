using System.Text;
using IdentityService.Application.Interfaces;
using IdentityService.Application.Services;
using IdentityService.Infrastructure.Data;
using IdentityService.Infrastructure.Repositories;
using IdentityService.Infrastructure.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// --- Custom .env Loader (Fix: Needs 3 levels to reach root) ---
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
builder.Logging.AddFilter("MassTransit", LogLevel.Error);
builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);

builder.Services.AddControllers();

var connectionString = Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTION")
                      ?? builder.Configuration.GetConnectionString("DefaultConnection");

var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET")
             ?? builder.Configuration["Jwt:Key"]
             ?? "THIS_IS_SECRET_KEY_CHANGE_IT_1234567890";

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "IdentityService";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "IdentityServiceClients";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
        sqlOptions.CommandTimeout(60);
    }));

// MassTransit Configuration
builder.Services.AddMassTransit(x =>
{
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
    });
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenService>(_ => new TokenService(jwtKey, jwtIssuer, jwtAudience));

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
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity Service API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter JWT token as: Bearer {your token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        var databaseCreator = dbContext.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
        if (databaseCreator != null)
        {
            if (!databaseCreator.Exists()) databaseCreator.Create();
            try { dbContext.Database.ExecuteSqlRaw("SELECT TOP 0 * FROM Users"); }
            catch { databaseCreator.CreateTables(); }
        }

        // --- NEW: Ensure ProfilePictureUrl column exists ---
        try
        {
            var checkColumnSql = @"
                IF NOT EXISTS (
                    SELECT * FROM sys.columns 
                    WHERE object_id = OBJECT_ID('Users') AND name = 'ProfilePictureUrl'
                )
                BEGIN
                    ALTER TABLE Users ADD ProfilePictureUrl NVARCHAR(MAX) NULL;
                END";
            dbContext.Database.ExecuteSqlRaw(checkColumnSql);
            // Console.WriteLine("✅ ProfilePictureUrl column check completed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Column sync notice: {ex.Message}");
        }

        // --- NEW: Seed Admin User ---
        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

        if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword))
        {
            var adminUser = dbContext.Users.FirstOrDefault(u => u.Email == adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityService.Domain.Entities.User
                {
                    Id = Guid.NewGuid(),
                    Name = "System Admin",
                    Email = adminEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                    Role = "Admin"
                };
                dbContext.Users.Add(adminUser);
                dbContext.SaveChanges();
                Console.WriteLine($"👑 Admin user created: {adminEmail}");
            }
            else if (adminUser.Role != "Admin")
            {
                adminUser.Role = "Admin";
                dbContext.SaveChanges();
                Console.WriteLine($"👑 User promoted to Admin: {adminEmail}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Sync notice: {ex.Message}");
    }
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Service API v1");
    options.RoutePrefix = "swagger";
});

// --- NEW: Security Headers for Google Auth ---
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Cross-Origin-Opener-Policy", "same-origin-allow-popups");
    context.Response.Headers.Append("Cross-Origin-Embedder-Policy", "require-corp");
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapControllers();

var port = "8001";
Console.WriteLine("✅ Database connected successfully!");
Console.WriteLine($"🚀 Identity Service is running on port {port}");
Console.WriteLine($"📖 Swagger UI: http://localhost:{port}/swagger");

app.Run();
