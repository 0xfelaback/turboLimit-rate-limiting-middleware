namespace RateLimiterMiddleware.src;

public sealed class RequireRateLimiterMetadata;

public static class EndpointRateLimiterExtensions
{
    public static TBuilder RequireRateLimiter<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.WithMetadata(new RequireRateLimiterMetadata());
        return builder;
    }
}
