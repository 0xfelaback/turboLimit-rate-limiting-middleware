namespace RateLimiterMiddleware.src;

public static class ExtensionClassBuilder
{
    public static IServiceCollection RateLimiterMiddlewareBuilderConfig(this IServiceCollection services, Action<RateLimitOptions> configOptions)
    {
    RateLimitOptions options = new RateLimitOptions();
    configOptions(options);
    if (string.IsNullOrWhiteSpace(options.TokenRefillPeriod))
        {
            throw new ArgumentNullException(nameof(options.TokenRefillPeriod),$"You must provide a valid token refill period");
        }
    services.AddSingleton(options);
    return services;
    }
}
