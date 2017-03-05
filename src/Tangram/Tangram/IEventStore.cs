using System;
using System.Collections.Generic;

namespace Tangram
{
    public interface IEventStore
    {
        IEnumerable<IEvent> Save<TAggregate>(TAggregate aggregate) 
            where TAggregate : IAggregate;

        IEnumerable<IEvent> Save<TAggregate, TSnapshot>(TAggregate aggregate)
            where TAggregate : ISnapshotAggregate<TSnapshot>
            where TSnapshot : IAggregateSnapshot;
       
        TAggregate GetById<TAggregate>(Guid id) 
            where TAggregate : IAggregate, new();

        TAggregate GetById<TAggregate, TSnapshot>(Guid id)
            where TAggregate : ISnapshotAggregate<TSnapshot>, new()
            where TSnapshot : IAggregateSnapshot;
    }
}
