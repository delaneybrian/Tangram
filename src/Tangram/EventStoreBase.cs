using System;
using System.Collections.Generic;
using System.Linq;

namespace Tangram
{
    public abstract class EventStoreBase
    {
        public abstract IEnumerable<IEvent> Save<TAggregate>(TAggregate aggregate)
            where TAggregate : IAggregate;

        public abstract IEnumerable<IEvent> Save<TAggregate, TSnapshot>(TAggregate aggregate)
            where TAggregate : ISnapshotAggregate<TSnapshot>
            where TSnapshot : IAggregateSnapshot;

        public abstract TAggregate GetById<TAggregate>(Guid id)
            where TAggregate : IAggregate, new();

        public abstract TAggregate GetById<TAggregate, TSnapshot>(Guid id)
            where TAggregate : ISnapshotAggregate<TSnapshot>, new()
            where TSnapshot : IAggregateSnapshot;

        protected int CalculateExpectedVersion<T>(IAggregate eventSourcedAggregate, List<T> events)
        {
            var expectedVersion = eventSourcedAggregate.Version - events.Count;
            return expectedVersion;
        }

        protected TResult BuildAggregate<TResult>(IEnumerable<IEvent> events) where TResult : IAggregate, new()
        {
            var result = new TResult();

            foreach (var evt in events)
            {
                result.ApplyEvent(evt);
            }

            return result;
        }

        protected bool SnapshotRequired(IEnumerable<IEvent> events, int snapshotFrequency)
        {
            if (snapshotFrequency <= 0)
                return false;

            return events.Any(e => 
                e.Version > 0 && 
                e.Version % snapshotFrequency == 0);
        }
    }
}
