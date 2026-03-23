namespace RateLimiterMiddleware.src;

public static class ExtensionClassApp
{
    public static IApplicationBuilder UseRateLimiterMiddleware(this IApplicationBuilder builder) => builder.UseMiddleware<RateLimiterMiddleware>();
}
