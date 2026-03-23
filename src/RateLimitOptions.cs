namespace RateLimiterMiddleware.src;

public class RateLimitOptions
{
    //for token bucket
    public const string SectionName = "RateLimitingSettings";

    public int TokenLimit {get; set;}
    public int TokensPerPeriod {get; set;}
    public string TokenRefillPeriod {get; set;} = null!;
    public int QueueLimit {get; set;}
}