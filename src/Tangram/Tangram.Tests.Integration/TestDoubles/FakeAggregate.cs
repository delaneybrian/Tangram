using System;
using System.Collections.Generic;

namespace Tangram.Tests.Integration.TestDoubles
{
    public class FakeAggregate : AggregateBase, ISnapshotAggregate<FakeAggregateSnapshot>
    {
        public FakeAggregate()
        {
            RegisterHandler<FakeAggregateCreated>(Apply);
            RegisterHandler<FakeAggregateUpdated>(Apply);
        }

        private FakeAggregate(Guid id, string initialText)
            : this()
        {
            RaiseEvent(new FakeAggregateCreated(id, initialText));
        }

        public string Text { get; private set; }

        public List<Tuple<string, int>> Updates { get; private set; }

        public int NumberOfUpdates { get; private set; }

        public static FakeAggregate Create(Guid id, string initialText)
        {
            return new FakeAggregate(id, initialText);
        }

        public void UpdateText(string newText, int number)
        {
            RaiseEvent(new FakeAggregateUpdated(Id, newText, number));
        }

        private void Apply(FakeAggregateCreated evt)
        {
            Id = evt.AggregateId;
            Text = evt.InitialText;
            Updates = new List<Tuple<string, int>>();
            NumberOfUpdates = 0;
        }

        private void Apply(FakeAggregateUpdated evt)
        {
            Text = evt.Text;
            Updates.Add(new Tuple<string, int>(evt.Text, evt.Number));
            NumberOfUpdates++;
        }

        public FakeAggregateSnapshot ToSnapshot()
        {
            return new FakeAggregateSnapshot
            {
                AggregateId = Id,
                Version = Version,
                Text = Text,
                NumberOfUpdates = NumberOfUpdates
            };
        }

        public void RestoreFromSnapshot(FakeAggregateSnapshot snapshot)
        {
            Id = snapshot.AggregateId;
            Version = snapshot.Version;
            Text = snapshot.Text;
            Updates = new List<Tuple<string, int>>();
            NumberOfUpdates = snapshot.NumberOfUpdates;
        }
    }
}
