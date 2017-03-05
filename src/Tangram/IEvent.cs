using System;

namespace Tangram
{
    public interface IEvent
    {
        Guid AggregateId { get; }

        int Version { get; set; }
    }
}
