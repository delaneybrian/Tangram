using System;

namespace Tangram.Exceptions
{
    public class AggregateConflictException : Exception
    {
        public AggregateConflictException(Guid id, int expected, int actual)
            : base($"Conflict while writing aggregate id {id}: Expected version {expected} but was version {actual}")
        {
        }
    }
}
