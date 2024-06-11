using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedisCacheResearch.Infrastructure.Redis.Options;

namespace RedisCacheResearch.Infrastructure.Redis;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var redisOptions = new RedisClusterOptions();
        configuration.GetSection(nameof(RedisClusterOptions)).Bind(redisOptions);

        var redisConfig = new RedisClusterConfiguration(redisOptions);
        
        services.AddSingleton(redisConfig);
        services.AddSingleton(redisOptions);

        return services;
    }
}