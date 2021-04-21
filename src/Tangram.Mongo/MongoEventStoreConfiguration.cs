namespace Tangram.Mongo
{
    public class MongoEventStoreConfiguration : IMongoEventStoreConfiguration
    {
        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }

        public string EventsCollectionName { get; set; }

        public string SnapshotsCollectionName { get; set; }
    }
}
