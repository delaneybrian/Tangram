using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tangram.Exceptions;

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
            var mongoClient = _connectionFactory.CreateConnection();

            return SaveEvents(mongoClient, aggregate);
        }

        public override IEnumerable<IEvent> Save<TAggregate, TSnapshot>(TAggregate aggregate)
        {
            var mongoClient = _connectionFactory.CreateConnection();

            var evts = SaveEvents(mongoClient, aggregate).ToList();

            if (_snapshottingEnabled && SnapshotRequired(evts, _snapshotFrequency))
            {
                var snapshot = aggregate.ToSnapshot();
                var json = _serializer.Serialize(snapshot);

                var database = mongoClient.GetDatabase(_mongoEventStoreConfiguration.DatabaseName);

                var snapshotsCollection =
                    database.GetCollection<MongoEntity>(_mongoEventStoreConfiguration.SnapshotsCollectionName);

                var mongoEntity = new MongoEntity
                {
                    Id = aggregate.Id,
                    Version = snapshot.Version,
                    Payload = _serializer.Serialize(snapshot)
                };

                snapshotsCollection
                    .InsertOne(mongoEntity);
            }

            return evts;
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

        private IEnumerable<IEvent> SaveEvents(IMongoClient mongoClient, IAggregate aggregate)
        {
            if (!aggregate.UncommitedEvents().Any())
            {
                return new List<IEvent>();
            }

            var evtsToSave = aggregate.UncommitedEvents().ToList();
            var expected = CalculateExpectedVersion(aggregate, evtsToSave);

            var database = mongoClient.GetDatabase(_mongoEventStoreConfiguration.DatabaseName);

            var eventsCollection = database.GetCollection<MongoEntity>(_mongoEventStoreConfiguration.EventsCollectionName);

            using (var session = mongoClient.StartSession())
            {
                try
                {
                    var current = eventsCollection
                        .AsQueryable()
                        .Count(x => x.Id == aggregate.Id) - 1;

                    if (expected != current)
                    {
                        throw new AggregateConflictException(aggregate.Id, expected, current);
                    }

                    var mongoEventsToSave = evtsToSave.Select(e => new MongoEntity
                    {
                        Id = e.AggregateId,
                        Version = e.Version,
                        Payload = _serializer.Serialize(e)
                    });

                    eventsCollection
                            .InsertMany(session, mongoEventsToSave);

                    session.CommitTransaction();
                }
                catch (Exception)
                {
                    session.AbortTransaction();

                    throw;
                }
            }

            return aggregate.UncommitedEvents();
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
