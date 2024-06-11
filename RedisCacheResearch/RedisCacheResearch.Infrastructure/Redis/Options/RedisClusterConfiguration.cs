using System.Net;

namespace RedisCacheResearch.Infrastructure.Redis.Options;

public class RedisClusterConfiguration
{
    private string[] _shardsConnStr;

    public RedisClusterConfiguration(RedisClusterOptions options)
    {
        _shardsConnStr = ParseForEveryShard(options.ConnectionString);
    }

    public string GetShardConnectionString(int shardNumber)
    {
        if (shardNumber > _shardsConnStr.Length)
            throw new ArgumentOutOfRangeException(
                $"Incorrect shard number. Possible values from 1 to {_shardsConnStr.Length}");

        return _shardsConnStr[shardNumber];
    }

    private string[] ParseForEveryShard(string commonConnStr)
    {
        var connections = commonConnStr.Split(',');

        foreach (var connection in connections)
            if (ValidateConnectionString(connection) is false)
                throw new ArgumentException($"Failed to parse IP address from: '{connection}'");

        return connections;
    }

    private bool ValidateConnectionString(string shardConnString)
    {
        var asIpV4Address = shardConnString.Replace("localhost", "127.0.0.1");

        return IPEndPoint.TryParse(asIpV4Address, out _);
    }
}