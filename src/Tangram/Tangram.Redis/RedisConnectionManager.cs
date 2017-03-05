using StackExchange.Redis;

namespace Tangram.Redis
{
    public class RedisConnectionManager : IRedisConnectionManager
    {
        public RedisConnectionManager(string redisConnectionString)
        {
            Redis = ConnectionMultiplexer.Connect(redisConnectionString);
        }

        public ConnectionMultiplexer Redis { get; }
    }
}
