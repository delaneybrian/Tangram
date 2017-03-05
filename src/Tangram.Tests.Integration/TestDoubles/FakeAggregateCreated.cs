using System;

namespace Tangram.Tests.Integration.TestDoubles
{
    public class FakeAggregateCreated : IEvent
    {
        public FakeAggregateCreated(Guid aggregateId, string initialText)
        {
            AggregateId = aggregateId;
            InitialText = initialText;
        }

        public Guid AggregateId { get; }

        public int Version { get; set; }

        public string InitialText { get; set; }
    }
}
