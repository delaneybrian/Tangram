using System;
using System.Runtime.Serialization;
using Tangram;

namespace MongoSample.Events
{
    [DataContract]
    public class CreatePerson : IEvent
    {
        [DataMember]
        public Guid AggregateId { get; set; }

        [DataMember]
        public int Version { get; set; }

        [DataMember]
        public int Age { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }


        [DataMember]
        public DateTime CreatedAtUtc { get; set; }

        [DataMember]
        public Guid CreatedByUserId { get; set; }
    }
}
