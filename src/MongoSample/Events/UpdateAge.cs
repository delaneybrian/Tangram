using System;
using System.Runtime.Serialization;
using Tangram;

namespace MongoSample.Events
{
    [DataContract]
    public class UpdateAge : IEvent
    {
        [DataMember] 
        public Guid AggregateId { get; set; }

        [DataMember] 
        public int Version { get; set; }

        [DataMember] 
        public int Age { get; set; }

        [DataMember]
        public DateTime UpdatedAtUtc { get; set; }

        [DataMember]
        public Guid UpdatedByUserId { get; set; }
    }
}
