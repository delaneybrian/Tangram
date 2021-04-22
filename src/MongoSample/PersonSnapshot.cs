using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Tangram;

namespace MongoSample
{
    [DataContract]
    public class PersonSnapshot : IAggregateSnapshot
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
        public int Age { get; set; }

        [DataMember]
        public DateTime CreatedAtUtc { get; set; }

        [DataMember]
        public DateTime LastUpdatedAtUtc { get; set; }

        [DataMember]
        public Guid CreatedByUserId { get; set; }

        [DataMember]
        public ICollection<Guid> UpdatedByUserIds { get; set; }
            = new List<Guid>();
    }
}
