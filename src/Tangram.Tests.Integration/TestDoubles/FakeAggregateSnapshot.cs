using System;

namespace Tangram.Tests.Integration.TestDoubles
{
    public class FakeAggregateSnapshot : IAggregateSnapshot
    {
        public Guid AggregateId { get; set; }

        public int Version { get; set; }

        public string Text { get; set; }

        public int NumberOfUpdates { get; set; }
    }
}
