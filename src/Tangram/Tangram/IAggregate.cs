using System;
using System.Collections.Generic;

namespace Tangram
{
    public interface IAggregate
    {
        Guid Id { get; }

        int Version { get; }

        IEnumerable<IEvent> UncommitedEvents();

        void ClearUncommitedEvents();

        void ApplyEvent(IEvent @event);
    }
}
