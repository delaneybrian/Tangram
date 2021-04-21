using MongoDB.Driver;

namespace Tangram.Mongo
{
    public interface IConnectionFactory
    {
        MongoClient CreateConnection();
    }
}
