
namespace Tangram
{
    public interface ISnapshotAggregate<TSnapshot> : IAggregate where TSnapshot : IAggregateSnapshot
    {
        TSnapshot ToSnapshot();

        void RestoreFromSnapshot(TSnapshot snapshot);
    }
}
