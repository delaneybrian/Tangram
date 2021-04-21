namespace Tangram.Sql
{
    public class SqlEventStoreConfiguration : ISqlEventStoreConfiguration
    {
        public string ConnectionString { get; set; }

        public string EventsTableName { get; set; }

        public string SnapshotsTableName { get; set; }
    }
}
