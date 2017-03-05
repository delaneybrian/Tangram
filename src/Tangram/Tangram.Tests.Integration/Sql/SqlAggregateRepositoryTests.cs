using System.Configuration;
using NUnit.Framework;
using Tangram.Json;
using Tangram.Sql;

namespace Tangram.Tests.Integration.Sql
{
    [TestFixture, Explicit]
    public class SqlAggregateRepositoryTests
    {
        private TestContext _context;

        [SetUp]
        public void Setup()
        {
            _context = new TestContext();
        }

        [Test]
        public void Save_Aggregate_PersistsData()
        {
            _context
                .ArrangeWithSnapshotting()
                .ActSaveConflictingUpdates()
                .AssertAggregateStoredCorrectly();
        }

        [Test]
        public void Save_SnapshotAggregate_PersistsData()
        {
            _context
                .ArrangeWithSnapshotting()
                .ActSaveSnapshotAggregateConflictingUpdates()
                .AssertAggregateStoredCorrectly();
        }

        [Test]
        public void Save_SnapshotAggregate_PersistsData_SnapshotsDisabled()
        {
            _context
                .ArrangeWithNoSnapshotting()
                .ActSaveSnapshotAggregateConflictingUpdates()
                .AssertAggregateStoredCorrectly();
        }

        private class TestContext : EventStoreTestContext
        {
            private readonly SqlEventStoreConfiguration _evtStoreConfig;

            public TestContext()
            {
                _evtStoreConfig = new SqlEventStoreConfiguration()
                {
                    ConnectionString = ConfigurationManager.ConnectionStrings["Tangram:SqlConnectionString"].ToString(),
                    EventsTableName = "events",
                    SnapshotsTableName = "snapshots"
                };
            }

            public TestContext ArrangeWithSnapshotting()
            {
                Sut = new SqlEventStore(new ConnectionFactory(_evtStoreConfig), _evtStoreConfig, new JsonSerializer(), 10);

                return this;
            }

            public TestContext ArrangeWithNoSnapshotting()
            {
                Sut = new SqlEventStore(new ConnectionFactory(_evtStoreConfig), _evtStoreConfig, new JsonSerializer());

                return this;
            }
        }
    }
}
