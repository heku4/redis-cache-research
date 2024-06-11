using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using RedisCacheResearch.Infrastructure.Redis.Options;
using RedisCacheResearch.Presentation.Controllers.Contracts;
using RedisCacheResearch.Presentation.Controllers.Contracts.GetValue;
using RedisCacheResearch.Presentation.Controllers.Contracts.Write;
using StackExchange.Redis;

namespace RedisCacheResearch.Presentation.Controllers;

[ApiController]
public class CacheController : ControllerBase
{
    private readonly ConnectionMultiplexer _redis;
    private readonly RedisClusterConfiguration _clusterConfiguration;
    private readonly ILogger<CacheController> _logger;

    public CacheController(RedisClusterConfiguration clusterConfiguration, ILogger<CacheController> logger)
    {
        _clusterConfiguration = clusterConfiguration;
        _redis = ConnectionMultiplexer.Connect(_clusterConfiguration.GetShardConnectionString(1));
        _logger = logger;
    }

    [HttpGet(nameof(GetAllFromShard))]
    public async IAsyncEnumerable<string> GetAllFromShard([FromQuery] int shardNumber,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var redis =
            await ConnectionMultiplexer.ConnectAsync(_clusterConfiguration.GetShardConnectionString(shardNumber));
        var redisDb = redis.GetDatabase();

        foreach (var num in Enumerable.Range(0, 128))
        {
            yield return await redisDb.StringGetAsync(num.ToString());
        }
    }

    [HttpGet(nameof(Get))]
    public async Task<GetValueFromRedisResponse> Get([FromQuery] GetValueFromRedisQuery query,
        CancellationToken cancellationToken)
    {
        var redisDb = _redis.GetDatabase();

        var result = await redisDb.StringGetAsync(query.Key.ToString());

        return new GetValueFromRedisResponse(result.ToString());
    }

    [HttpPost(nameof(WriteToRedis))]
    public async Task<WriteToRedisResponse> WriteToRedis(WriteToRedisQuery query, CancellationToken cancellationToken)
    {
        var redisDb = _redis.GetDatabase();

        await redisDb.StringSetAsync(query.Key.ToString(), query.Value);

        return new WriteToRedisResponse();
    }
}