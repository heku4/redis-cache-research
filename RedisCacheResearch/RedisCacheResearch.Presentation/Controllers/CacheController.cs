using Microsoft.AspNetCore.Mvc;
using RedisCacheResearch.Presentation.Controllers.Contracts;
using RedisCacheResearch.Presentation.Controllers.Contracts.GetValue;
using RedisCacheResearch.Presentation.Controllers.Contracts.Write;
using StackExchange.Redis;

namespace RedisCacheResearch.Presentation.Controllers;

[ApiController]
public class CacheController : ControllerBase
{
    private readonly ConnectionMultiplexer _redis;
    private readonly ILogger<CacheController> _logger;

    public CacheController(ILogger<CacheController> logger)
    {
        //("server1:6381, server2:6382, server3:6383, server4:6384, server5:6385, server6:6386");
        _redis = ConnectionMultiplexer.Connect("localhost:6379");
        _logger = logger;
    }

    [HttpGet(nameof(GetFromRedis))]
    public async Task<GetValueFromRedisResponse> GetFromRedis([FromQuery]GetValueFromRedisQuery query, CancellationToken cancellationToken)
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