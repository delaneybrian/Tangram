using System;

namespace Tangram.Exceptions
{
    public class AggregateNotFoundException : Exception
    {
        public AggregateNotFoundException(Type t, Guid id)
            : this($"Could not found aggregate of type {t.Name} and id {id}")
        {
        }

        public AggregateNotFoundException(string message)
            : base(message)
        {
        }
    }
}
