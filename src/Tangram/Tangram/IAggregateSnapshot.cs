using System;

namespace Tangram
{
    public interface IAggregateSnapshot
    {
        Guid AggregateId { get; set; }

        int Version { get; set; }
    }
}
