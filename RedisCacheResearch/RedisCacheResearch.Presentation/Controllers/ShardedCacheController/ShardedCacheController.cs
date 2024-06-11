using Microsoft.AspNetCore.Mvc;
using RedisCacheResearch.Infrastructure.Redis.Options;
using RedisCacheResearch.Presentation.Controllers.Contracts.GetValue;
using StackExchange.Redis;

namespace RedisCacheResearch.Presentation.Controllers.ShardedCacheController;

[ApiController]
public class ShardedCacheController : ControllerBase
{
    private readonly RedisClusterOptions _redisClusterOptions;

    private readonly ILogger<ShardedCacheController> _logger;

    public ShardedCacheController(RedisClusterOptions redisClusterOptions, ILogger<ShardedCacheController> logger)
    {
        _redisClusterOptions = redisClusterOptions;
        _logger = logger;
    }

    [HttpGet(nameof(GetFromRedis))]
    public async Task<string> GetFromRedis()
    {
        await using var cnm = await ConnectionMultiplexer.ConnectAsync(_redisClusterOptions.ConnectionString);
        var db = cnm.GetDatabase();

        var testResult = await db.PingAsync();
        _logger.LogInformation($"Ping: {testResult}");
        return $"Endpoints: {string.Join(",", cnm.GetEndPoints().Select(x => x.ToString()))}";
    }

    [HttpPost(nameof(InfillShards))]
    public async Task InfillShards()
    {
        try
        {
            await using var cnm = await ConnectionMultiplexer.ConnectAsync(_redisClusterOptions.ConnectionString);

            for (var i = 0; i < cnm.GetEndPoints().Length; i++)
            {
                var db = cnm.GetDatabase(i);
                var tasks = Enumerable
                    .Range(0, 10)
                    .Select(async (x) => await db.StringSetAsync(x.ToString(), x.ToString()));

                await db.StringSetAsync(128.ToString(), "Final");

                await Task.WhenAll(tasks);
            }
        }
        catch (Exception e)
        {
            //
        }
    }
}