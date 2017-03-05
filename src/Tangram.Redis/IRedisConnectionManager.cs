using StackExchange.Redis;

namespace Tangram.Redis
{
    public interface IRedisConnectionManager
    {
        ConnectionMultiplexer Redis { get; }
    }
}
