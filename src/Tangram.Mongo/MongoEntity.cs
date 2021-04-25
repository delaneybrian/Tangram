using System;
using System.Runtime.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace Tangram.Mongo
{
    [DataContract]
    [BsonIgnoreExtraElements]
    internal class MongoEntity
    {
        [BsonConstructor]
        public MongoEntity()
        { 
        }

        [DataMember]
        public Guid AggregateId { get; set; }

        [DataMember]
        public int Version { get; set; }

        [DataMember]
        public string Payload { get; set; }
    }
}
