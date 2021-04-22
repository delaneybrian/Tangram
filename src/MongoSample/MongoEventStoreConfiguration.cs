using Tangram.Mongo;

namespace MongoSample
{
    public class MongoEventStoreConfiguration : IMongoEventStoreConfiguration
    {
        public string ConnectionString => "mongodb://multiple-ranker-test-mongo:EAk4XbpPYIsp8ctgt83HdGfQg1m9Gfeuo3McSo5euYCNctoZ4GSyX2gBcAbC7uDKsOBexVNJF8sALoebBzczTA==@multiple-ranker-test-mongo.mongo.cosmos.azure.com:10255/?ssl=true&replicaSet=globaldb&retrywrites=false&maxIdleTimeMS=120000&appName=@multiple-ranker-test-mongo@";

        public string DatabaseName => "tangramtest";

        public string EventsCollectionName => "events";

        public string SnapshotsCollectionName => "snapshots";
    }
}
