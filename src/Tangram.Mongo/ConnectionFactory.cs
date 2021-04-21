using MongoDB.Driver;

namespace Tangram.Mongo
{
    public class ConnectionFactory : IConnectionFactory
    {
        private readonly IMongoEventStoreConfiguration _mongoEventStoreConfiguration;

        public ConnectionFactory(IMongoEventStoreConfiguration mongoEventStoreConfiguration)
        {
            _mongoEventStoreConfiguration = mongoEventStoreConfiguration;
        }

        public MongoClient CreateConnection()
        {
            return new MongoClient(
                _mongoEventStoreConfiguration.ConnectionString
            );
        }
    }
}
