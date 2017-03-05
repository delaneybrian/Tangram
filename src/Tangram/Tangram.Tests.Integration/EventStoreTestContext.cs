using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Tangram.Exceptions;
using Tangram.Tests.Integration.TestDoubles;

namespace Tangram.Tests.Integration
{
    internal abstract class EventStoreTestContext
    {
        private readonly Guid _aggregateId;
        private readonly FakeAggregate _aggregate;
        private readonly int _taskCount;

        protected IFixture Fixture;
        protected IEventStore Sut;

        protected EventStoreTestContext()
        {
            Fixture = new Fixture();

            _taskCount = 100;
            _aggregateId = Guid.NewGuid();
            _aggregate = FakeAggregate.Create(_aggregateId, "initial text");
        }

        public EventStoreTestContext ActSaveConflictingUpdates()
        {
            Sut.Save(_aggregate);

            Func<Guid, string, Task> makeUpdateTask = (id, s) =>
                Task.Run(() =>
                {
                    var saved = false;

                    while (!saved)
                    {
                        var a = Sut.GetById<FakeAggregate>(id);

                        a.UpdateText($"{s}", 1);
                        a.UpdateText($"{s}", 2);
                        a.UpdateText($"{s}", 3);

                        try
                        {
                            Sut.Save(a);
                            saved = true;
                        }
                        catch (AggregateConflictException ex)
                        {
                            Debug.WriteLine($"Conflict: {ex.Message}");
                        }
                    }
                });

            var stopwatch = Stopwatch.StartNew();

            var updateTasks = new Task[_taskCount];

            Parallel.ForEach(
                Enumerable.Range(0, _taskCount),
                (i) => updateTasks[i] = makeUpdateTask(_aggregateId, i.ToString()));

            Task.WaitAll(updateTasks);

            stopwatch.Stop();

            Debug.WriteLine($"Completed writing {_taskCount * 3} events in {stopwatch.Elapsed}");

            return this;
        }

        public EventStoreTestContext ActSaveSnapshotAggregateConflictingUpdates()
        {
            Sut.Save<FakeAggregate, FakeAggregateSnapshot>(_aggregate);

            Func<Guid, string, Task> makeUpdateTask = (id, s) =>
                Task.Run(() =>
                {
                    var saved = false;

                    while (!saved)
                    {
                        var a = Sut.GetById<FakeAggregate, FakeAggregateSnapshot>(id);

                        a.UpdateText($"{s}", 1);
                        a.UpdateText($"{s}", 2);
                        a.UpdateText($"{s}", 3);

                        try
                        {
                            Sut.Save<FakeAggregate, FakeAggregateSnapshot>(a);
                            saved = true;
                        }
                        catch (AggregateConflictException ex)
                        {
                            Debug.WriteLine($"Conflict: {ex.Message}");
                        }
                    }
                });

            var stopwatch = Stopwatch.StartNew();

            var updateTasks = new Task[_taskCount];

            Parallel.ForEach(
                Enumerable.Range(0, _taskCount),
                (i) => updateTasks[i] = makeUpdateTask(_aggregateId, i.ToString()));

            Task.WaitAll(updateTasks);

            stopwatch.Stop();

            Debug.WriteLine($"Completed writing {_taskCount * 3} events in {stopwatch.Elapsed}");

            return this;
        }

        public void AssertAggregateStoredCorrectly()
        {
            var stopwatch = Stopwatch.StartNew();

            var aggregate = Sut.GetById<FakeAggregate>(_aggregateId);

            stopwatch.Stop();

            Debug.WriteLine($"Retrieved {_taskCount * 3} events in {stopwatch.Elapsed}");

            var lastText = string.Empty;
            var count = 1;

            var totalUpdates = _taskCount * 3;

            Assert.GreaterOrEqual(aggregate.Updates.Count, totalUpdates);

            foreach (var update in aggregate.Updates)
            {
                if (lastText != update.Item1)
                {
                    count = 1;
                    Assert.AreEqual(count, update.Item2);
                    lastText = update.Item1;
                }
                else
                {
                    count++;
                    Assert.AreEqual(count, update.Item2);
                }
            }
        }
    }
}
