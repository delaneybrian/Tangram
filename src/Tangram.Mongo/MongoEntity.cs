using System;
using System.Runtime.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace Tangram.Mongo
{
    [DataContract]
    internal class MongoEntity
    {
        [BsonConstructor]
        public MongoEntity()
        { 
        }

        [BsonId]
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public int Version { get; set; }

        [DataMember]
        public string Payload { get; set; }
    }
}
