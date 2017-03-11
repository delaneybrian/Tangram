using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;
using Tangram.Exceptions;

namespace Tangram.Redis
{
    public class RedisEventStore : EventStoreBase, IEventStore
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly ISerializer _serializer;
        private readonly int _snapshotFrequency;
        private readonly bool _snapshottingEnabled;

        public RedisEventStore(IRedisConnectionManager connectionManager, ISerializer serializer)
        {
            _serializer = serializer;
            _redis = connectionManager.Redis;
            _snapshottingEnabled = false;
            _snapshotFrequency = 0;
        }

        public RedisEventStore(IRedisConnectionManager connectionManager, ISerializer serializer, int snapshotFrequency)
        {
            if (snapshotFrequency <= 0)
                throw new ArgumentException("Snapshot frequency must be greater than 0 if set", nameof(snapshotFrequency));

            _serializer = serializer;
            _snapshotFrequency = snapshotFrequency;
            _redis = connectionManager.Redis;
            _snapshottingEnabled = true;
        }

        public override IEnumerable<IEvent> Save<TAggregate>(TAggregate aggregate)
        {
            return SaveEvents(_redis.GetDatabase(), aggregate);
        }

        public override IEnumerable<IEvent> Save<TAggregate, TSnapshot>(TAggregate aggregate)
        {
            var db = _redis.GetDatabase();

            var evts = SaveEvents(_redis.GetDatabase(), aggregate).ToList();

            if (_snapshottingEnabled && SnapshotRequired(evts, _snapshotFrequency))
            {
                var json = _serializer.Serialize(aggregate.ToSnapshot());

                db.StringSet(MakeSnapshotKey(aggregate.GetType(), aggregate.Id), json);
            }

            return evts;
        }

        public override TAggregate GetById<TAggregate>(Guid id)
        {
            var db = _redis.GetDatabase();

            Func<IAggregate> makeAggregate = () => new TAggregate();

            return (TAggregate)GetById(db, id, typeof(TAggregate), makeAggregate);
        }

        public override TAggregate GetById<TAggregate, TSnapshot>(Guid id)
        {
            var db = _redis.GetDatabase();

            Func<IAggregate> makeAggregate = () =>
            {
                var aggregate = new TAggregate();

                if (_snapshottingEnabled)
                {
                    var snapshot = db.StringGet(MakeSnapshotKey(typeof (TAggregate), id));

                    if (snapshot != RedisValue.Null)
                    {
                        var aggregateSnapshot = (TSnapshot) _serializer.Deserialize<IAggregateSnapshot>(snapshot);
                        aggregate.RestoreFromSnapshot(aggregateSnapshot);
                    }
                }

                return aggregate;
            };

            return (TAggregate)GetById(db, id, typeof (TAggregate), makeAggregate);
        }

        private IEnumerable<IEvent> SaveEvents(IDatabase db, IAggregate aggregate)
        {
            if (!aggregate.UncommitedEvents().Any())
            {
                return new List<IEvent>();
            }

            var key = MakeKey(aggregate.GetType(), aggregate.Id);

            var evtsToSave = aggregate.UncommitedEvents().ToList();

            var expected = CalculateExpectedVersion(aggregate, evtsToSave);
            var current = db.HashGetAll(key).Length - 1;

            if (expected != current)
            {
                throw new AggregateConflictException(aggregate.Id, expected, current);
            }

            foreach (var e in evtsToSave)
            {
                var version = e.Version.ToString();
                var json = _serializer.Serialize(e);

                var success = db.HashSet(key, version, json, When.NotExists);

                if (!success)
                {
                    throw new AggregateConflictException(aggregate.Id, expected, current);
                }
            }

            return aggregate.UncommitedEvents();
        }

        private IAggregate GetById(IDatabase db, Guid id, Type type, Func<IAggregate> aggregateFunc)
        {
            var key = MakeKey(type, id);

            var data = db.HashGetAll(key);

            if (!data.Any())
            {
                throw new AggregateNotFoundException(type, id);
            }

            var aggregate = aggregateFunc();
            
            var events = data
                .Where(e => Convert.ToInt32(e.Name) > aggregate.Version)
                .OrderBy(e => Convert.ToInt32(e.Name))
                .Select(x => _serializer.Deserialize<IEvent>(x.Value));

            foreach (var e in events)
            {
                aggregate.ApplyEvent(e);
            }

            return aggregate;
        }

        private static RedisKey MakeKey(Type t, Guid id)
        {
            return $"{t.Name}:{id}";
        }

        private static RedisKey MakeSnapshotKey(Type t, Guid id)
        {
            return $"{t.Name}:Snapshot:{id}";
        }
    }
}
