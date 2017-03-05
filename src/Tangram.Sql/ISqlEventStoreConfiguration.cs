
namespace Tangram.Sql
{
    public interface ISqlEventStoreConfiguration
    {
        string ConnectionString { get; }

        string EventsTableName { get; }

        string SnapshotsTableName { get; }
    }
}
