using RateLimiterMiddleware.src;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var redisConnection = ConnectionMultiplexer.Connect("localhost:6379");
builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);
if (!builder.Configuration.GetSection("RateLimitingSettings").Exists())
{
    throw new InvalidOperationException("Configuration section 'RateLimitingSettings' is missing.");
}
builder.Services.Configure<RateLimitOptions>(
    builder.Configuration.GetSection(RateLimitOptions.SectionName)
);
builder.Services.AddScoped(sp => 
{
    var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
    return multiplexer.GetDatabase();
});
builder.Services.AddSingleton<IRateLimiterStore, RedisRateLimiterStore>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.UseRateLimiterMiddleware();

app.Run();
