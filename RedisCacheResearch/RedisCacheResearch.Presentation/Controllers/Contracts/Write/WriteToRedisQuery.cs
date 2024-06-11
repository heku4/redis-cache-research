namespace RedisCacheResearch.Presentation.Controllers.Contracts.Write;

public record WriteToRedisQuery(long Key, string Value);