using System;
using System.Runtime.Serialization;
using Tangram;

namespace MongoSample.Events
{
    [DataContract]
    public class UpdateName : IEvent
    {
        [DataMember]
        public Guid AggregateId { get; set; }

        [DataMember]
        public int Version { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public DateTime UpdatedAtUtc { get; set; }

        [DataMember]
        public Guid UpdatedByUserId { get; set; }
    }
}
