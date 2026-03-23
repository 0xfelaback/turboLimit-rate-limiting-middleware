namespace RateLimiterMiddleware.src;

public interface IRateLimiterStore
{
    Task<RateLimitResult> CheckRateLimitAsync(string key);
}

