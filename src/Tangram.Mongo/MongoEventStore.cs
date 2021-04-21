using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Tangram.Mongo
{
    public class MongoEventStore : EventStoreBase, IEventStore
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IMongoEventStoreConfiguration _mongoEventStoreConfiguration;
        private readonly ISerializer _serializer;
        private readonly int _snapshotFrequency;
        private readonly bool _snapshottingEnabled;

        public MongoEventStore(IConnectionFactory connectionFactory, IMongoEventStoreConfiguration mongoEventStoreConfiguration, ISerializer serializer)
        {
            _connectionFactory = connectionFactory;
            _mongoEventStoreConfiguration = mongoEventStoreConfiguration;
            _serializer = serializer;
            _snapshottingEnabled = false;
            _snapshotFrequency = 0;
        }

        public MongoEventStore(IConnectionFactory connectionFactory, IMongoEventStoreConfiguration mongoEventStoreConfiguration, ISerializer serializer, int snapshotFrequency)
        {
            if (snapshotFrequency <= 0)
                throw new ArgumentException("Snapshot frequency must be greater than 0 if set", nameof(snapshotFrequency));

            _connectionFactory = connectionFactory;
            _mongoEventStoreConfiguration = mongoEventStoreConfiguration;
            _serializer = serializer;
            _snapshotFrequency = snapshotFrequency;
            _snapshottingEnabled = true;
        }

        public override IEnumerable<IEvent> Save<TAggregate>(TAggregate aggregate)
        {
        }

        public override IEnumerable<IEvent> Save<TAggregate, TSnapshot>(TAggregate aggregate)
        {

        }

        public override TAggregate GetById<TAggregate>(Guid id)
        {
            var mongoClient = _connectionFactory.CreateConnection();

            var database = mongoClient.GetDatabase(_mongoEventStoreConfiguration.DatabaseName);

            var collection = database.GetCollection<MongoEntity>(_mongoEventStoreConfiguration.EventsCollectionName);

            return (TAggregate)GetById(collection, id, typeof(TAggregate), () => new TAggregate());
        }

        public override TAggregate GetById<TAggregate, TSnapshot>(Guid id)
        {
            var mongoClient = _connectionFactory.CreateConnection();

            var database = mongoClient.GetDatabase(_mongoEventStoreConfiguration.DatabaseName);

            var snapshotsCollection = database.GetCollection<MongoEntity>(_mongoEventStoreConfiguration.SnapshotsCollectionName);

            Func<IAggregate> makeAggregate = () =>
                {
                    var aggregate = new TAggregate();

                    if (!_snapshottingEnabled)
                        return aggregate;

                    var latestSnapshot = snapshotsCollection.AsQueryable()
                        .OrderBy(x => x.Version)
                        .FirstOrDefault();

                    if (latestSnapshot == null)
                        return aggregate;

                    var snapshot = (TSnapshot)_serializer.Deserialize<IAggregateSnapshot>(latestSnapshot.Payload.ToString());

                    aggregate.RestoreFromSnapshot(snapshot);

                    return aggregate;
                };

            var eventsCollection = database.GetCollection<MongoEntity>(_mongoEventStoreConfiguration.EventsCollectionName);

            return (TAggregate)GetById(eventsCollection, id, typeof(TAggregate), makeAggregate);
        }

        private IAggregate GetById(IMongoCollection<MongoEntity> collection, Guid id, Type type, Func<IAggregate> aggregateFunc)
        {
            var aggregate = aggregateFunc();

            var eventQueryable = collection.AsQueryable()
                .Where(evt => evt.Id == id && evt.Version < aggregate.Version)
                .OrderBy(e => e.Version);

            eventQueryable.ForEachAsync(evt =>
            {
                var deserializeEvent = _serializer.Deserialize<IEvent>(evt.Payload.ToString());

                aggregate.ApplyEvent(deserializeEvent);
            }).Wait();

            return aggregate;
        }
    }
}
