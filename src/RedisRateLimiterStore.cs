using Microsoft.Extensions.Options;
using StackExchange.Redis;


namespace RateLimiterMiddleware.src;

public class RedisRateLimiterStore : IRateLimiterStore
{
    private readonly RateLimitOptions _options;
    private readonly IDatabase _database;
    private readonly string _luaScript;
    public RedisRateLimiterStore(IOptions<RateLimitOptions> options, IConnectionMultiplexer redisConnection)
    {
        _options = options.Value;
        _database = redisConnection.GetDatabase();
        _luaScript = File.ReadAllText("/Users/great/Desktop/cs/RateLimiterMiddleware/src/script.lua");
    }

    public async Task<RateLimitResult> CheckRateLimitAsync(string userRedisKey)
    {
        RedisResult result = await _database.ScriptEvaluateAsync(_luaScript, [userRedisKey],[_options.TokenLimit, _options.TokensPerPeriod, DateTimeOffset.Now.ToUnixTimeSeconds()]);
        if (result.IsNull)
        {
            throw new KeyNotFoundException("Lua script returned null: Redis key or script result not found.");
        }
        var values = (RedisValue[])result!;
        bool allowed = (int)values[0] == 1;
        int remainingTokens = (int)values[1];

        RateLimitResult rateLimitResult = new RateLimitResult
        {
            isAllowed = allowed,
            remainingTokens = remainingTokens
        };
        return rateLimitResult;
    }
}

public class RateLimitResult
{
    public int remainingTokens {get; set;}
    public bool isAllowed {get; set;}
}

