local key = KEYS[1]
local max_tokens = tonumber(ARGV[1])
local refill_rate = tonumber(ARGV[2])
local current_time = tonumber(ARGV[3])

local bucket = redis.call('HMGET', key, 'tokens', 'last_refill')
local tokens = tonumber(bucket[1]) or max_tokens
local last_refill = tonumber(bucket[2]) or current_time

local time_passed = current_time - last_refill
local new_tokens = math.floor(time_passed * refill_rate)

tokens = math.min(max_tokens, tokens + new_tokens)

if tokens >= 1 then
    tokens = tokens - 1
    redis.call('HMSET', key, 'tokens', tokens, 'last_refill', current_time)
    return {1, tokens} 
else
    return {0, tokens} 
end