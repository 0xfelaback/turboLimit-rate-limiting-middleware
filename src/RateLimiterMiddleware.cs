namespace RateLimiterMiddleware.src;

public class RateLimiterMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRateLimiterStore _store;
    public RateLimiterMiddleware ( RequestDelegate next, IRateLimiterStore store)
    {
       _next = next;
       _store = store;
    }
    public async Task InvokeAsync(HttpContext httpContext)
    {
        var userIP = httpContext.Connection.RemoteIpAddress;
        if (userIP is null)
        {
            throw new NotFoundException("User is null");
        }
        RateLimitResult result = await _store.CheckRateLimitAsync(userIP.ToString());
        httpContext.Response.Headers.Append("X-RateLimit-Remaining", $"Request token remaining {result.remainingTokens}");
        if (result.isAllowed)
        {
            await _next(httpContext);
        } else
        {
            httpContext.Response.StatusCode = 429;
        }
    }
}

public class NotFoundException: Exception
{
    public NotFoundException(string message) : base(message) { }
}
