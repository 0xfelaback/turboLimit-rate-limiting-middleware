# RateLimiterMiddleware

A high-performance, asynchronous, and distributed Rate Limiter Middleware for ASP.NET Core applications.
Designed to prevent abuse of APIs by throttling incoming requests based on configurable algorithms.

Currently, this repository provides a distributed **Token Bucket** implementation using Redis and Lua scripting, which guarantees true atomicity across distributed environments and multiple nodes.

## Architecture Decisions

- **Middleware Design:** Built natively as ASP.NET Core middleware (`IMiddleware` / `RequestDelegate`) for seamless pipeline integration. 
- **Distributed Store:** Uses **Redis** as the primary backing store (`IRateLimiterStore`) for tokens, ensuring rate limits are accurately enforced across scaled-out API instances.
- **Atomicity via Lua:** The token evaluation (fetching current tokens, calculating elapsed time, refilling tokens, and consuming a token) is handled in a single atomic Lua script (`script.lua`) sent to Redis. This prevents race conditions under high concurrency.
- **Dependency Injection:** The core implementation allows straightforward DI configuration and minimal setup. Options are automatically bound via the built-in `IOptions` pattern.

## High Performance & Benchmark Results

The middleware has been rigorously tested using `bombardier` and `k6` to assure its capability to handle massive throughput with negligible overhead.

### Bombardier Benchmarks

Performance testing executed against a local API endpoint (`http://localhost:5068/`):

| Connections | Requests/Duration | Requests/sec (Avg) | Requests/sec (Max) | Latency (Avg) | Latency (Max) |
|---|---|---|---|---|---|
| 1 | 50 requests | 1,224.11 | 1,682.92 | 0.91ms | 10.59ms |
| 1 | 200 requests | 2,716.65 | 3,916.11 | 376.44µs | 4.35ms |
| 5 | 1,000 requests | 10,516.31 | 12,304.39 | 475.05µs | 6.32ms |
| 50 | 10 seconds | 47,510.39 | 56,862.83 | 1.05ms | 23.17ms |
| **100** | **50 seconds** | **54,522.30** | **72,443.94** | **1.83ms** | **44.88ms** |

*Note: Tested locally, yielding a massive sustained rate of ~54,000 requests per second with only ~1.8ms average latency in a high-concurrency scenario.*

### k6 Stress Tests

In addition to bombardier, `k6` was utilized for complex lifecycle and staging load tests:

#### Standard Load (50 VUs for 50s)
- **Total Requests:** 146,410
- **Throughput:** ~2,928 req/s
- **HTTP Request Duration (Avg):** 2.73ms
- **Successful Blocks (HTTP 429):** 145,810 requests efficiently rejected.

#### Heavy Stress Test (500 VUs for 1m)
- **Total Requests:** 2,355,640
- **Throughput:** ~39,260 req/s
- **HTTP Request Duration (Avg):** 5.74ms (extremely resilient under 500 concurrent connections)
- **Data Transfer:** 370 MB received, 165 MB sent.

These tests indicate the middleware gracefully limits traffic without crashing the server and absorbs heavy loads flawlessly.

## Future Plans

- **Fixed Window Algorithm:** In upcoming releases, I plan to introduce a `RateLimiterFixedWindowStore.cs` to allow developers a choice of algorithm. 
- **Algorithm Configurator:** Exposing the rate limiter algorithm as a configuration option via `appsettings.json`, allowing the user to select between Token Bucket, Fixed Window, and potentially Sliding Window algorithms.

## Usage

**1. Configuration (`appsettings.json`)**
```json
{
  "RateLimitingSettings": {
    "TokenLimit": 100,
    "TokensPerPeriod": 10,
    "TokenRefillPeriod": "1s",
    "QueueLimit": 0
  }
}
```

**2. Setup in `Program.cs`**
```csharp
using RateLimiterMiddleware.src;

var builder = WebApplication.CreateBuilder(args);

// Add dependencies
var redisConnection = ConnectionMultiplexer.Connect("localhost:6379");
builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);

builder.Services.Configure<RateLimitOptions>(
    builder.Configuration.GetSection("RateLimitingSettings")
);

builder.Services.AddSingleton<IRateLimiterStore, RedisRateLimiterStore>();

var app = builder.Build();

app.UseRateLimiterMiddleware();

app.Run();
```
