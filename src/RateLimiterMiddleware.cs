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
        var endpoint = httpContext.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<RequireRateLimiterMetadata>() is null)
        {
            await _next(httpContext);
            return;
        }

        var userIP = httpContext.Connection.RemoteIpAddress;
        if (userIP is null)
        {
            throw new NotFoundException("User is null");
        }
        string rateLimitKey = BuildRateLimitKey(httpContext, userIP.ToString());
        RateLimitResult result = await _store.CheckRateLimitAsync(rateLimitKey);
        httpContext.Response.Headers.Append("X-RateLimit-Remaining", $"Request token remaining {result.remainingTokens}");
        if (result.isAllowed)
        {
            await _next(httpContext);
        } else
        {
            httpContext.Response.StatusCode = 429;
        }
    }

    private static string BuildRateLimitKey(HttpContext httpContext, string userIp)
    {
        string method = httpContext.Request.Method;
        string endpointPath = httpContext.GetEndpoint() is RouteEndpoint routeEndpoint
            ? routeEndpoint.RoutePattern.RawText ?? httpContext.Request.Path.Value ?? "unknown"
            : httpContext.Request.Path.Value ?? "unknown";

        return $"{userIp}:{method}:{endpointPath}".ToLowerInvariant();
    }
}

public class NotFoundException: Exception
{
    public NotFoundException(string message) : base(message) { }
}
