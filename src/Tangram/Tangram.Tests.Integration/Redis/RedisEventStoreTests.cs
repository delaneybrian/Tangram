using System.Configuration;
using NUnit.Framework;
using Tangram.Json;
using Tangram.Redis;

namespace Tangram.Tests.Integration.Redis
{
    [TestFixture, Explicit]
    public class RedisEventStoreTests
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
            private readonly string _redisConnectionString;

            public TestContext()
            {
                _redisConnectionString = ConfigurationManager.ConnectionStrings["Tangram:RedisConnectionString"].ToString();
            }

            public TestContext ArrangeWithSnapshotting()
            {
                Sut = new RedisEventStore(new RedisConnectionManager(_redisConnectionString), new JsonSerializer(), 10);

                return this;
            }

            public TestContext ArrangeWithNoSnapshotting()
            {
                Sut = new RedisEventStore(new RedisConnectionManager(_redisConnectionString), new JsonSerializer());

                return this;
            }
        }
    }
}
