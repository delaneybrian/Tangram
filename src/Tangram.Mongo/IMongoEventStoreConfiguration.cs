namespace Tangram.Mongo
{
    public interface IMongoEventStoreConfiguration
    {
        string ConnectionString { get; }

        string DatabaseName { get; }

        string EventsCollectionName { get; }

        string SnapshotsCollectionName { get; }
    }
}
