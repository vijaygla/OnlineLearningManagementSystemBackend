var builder = WebApplication.CreateBuilder(args);

// Add YARP services and load configuration from appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseRouting();

app.MapGet("/", () => "Online Learning Management System API Gateway is running!");

// Map the reverse proxy middleware
app.MapReverseProxy();

var port = 8000;
Console.WriteLine($"🚀 API Gateway is running on port {port}");

// Dynamic display of routes based on configuration
var proxyConfig = builder.Configuration.GetSection("ReverseProxy:Clusters");
Console.WriteLine("🔗 Routing Overview:");
foreach (var cluster in proxyConfig.GetChildren())
{
    var address = cluster.GetSection("Destinations:destination1:Address").Value;
    Console.WriteLine($"  - {cluster.Key}: {address}");
}

app.Run($"http://0.0.0.0:{port}");
