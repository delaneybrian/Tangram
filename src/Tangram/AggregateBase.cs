using System;
using System.Collections.Generic;

namespace Tangram
{
    public class AggregateBase : IAggregate
    {
        private readonly List<IEvent> _uncommitedEvents = new List<IEvent>();
        private readonly Dictionary<Type, Action<IEvent>> _eventHandlers = new Dictionary<Type, Action<IEvent>>();

        public AggregateBase()
        {
            Version = -1;
        }

        public Guid Id { get; protected set; }

        public int Version { get; protected set; }

        public void RaiseEvent(IEvent evt)
        {
            ApplyEvent(evt);
            _uncommitedEvents.Add(evt);
        }

        public void ApplyEvent(IEvent evt)
        {
            var eventType = evt.GetType();

            if (_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType](evt);
            }

            evt.Version = ++Version;
        }

        public IEnumerable<IEvent> UncommitedEvents()
        {
            return _uncommitedEvents;
        }

        public void ClearUncommitedEvents()
        {
            _uncommitedEvents.Clear();
        }

        protected void RegisterHandler<T>(Action<T> eventHandler) where T : class
        {
            _eventHandlers.Add(typeof(T), o => eventHandler(o as T));
        }
    }
}
