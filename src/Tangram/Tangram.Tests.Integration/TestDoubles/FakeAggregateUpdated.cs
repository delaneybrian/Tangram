using System;

namespace Tangram.Tests.Integration.TestDoubles
{
    internal class FakeAggregateUpdated : IEvent
    {
        public FakeAggregateUpdated(Guid aggregateId, string text, int num)
        {
            AggregateId = aggregateId;
            Text = text;
            Number = num;
        }

        public Guid AggregateId { get; }

        public int Version { get; set; }

        public string Text { get; private set; }

        public int Number { get; set; }
    }
}
