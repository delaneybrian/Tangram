using Autofac;
using Tangram;
using Tangram.Json;
using Tangram.Mongo;

namespace MongoSample
{
    internal static class Bootstrapper
    {
        public static IContainer Bootstrap()
        {
            var builder = new ContainerBuilder();

            builder
                .RegisterType<MongoEventStoreConfiguration>()
                .As<IMongoEventStoreConfiguration>();

            builder
                .RegisterType<ConnectionFactory>()
                .As<IConnectionFactory>();

            builder
                .RegisterType<JsonSerializer>()
                .As<ISerializer>();

            builder
                .RegisterType<MongoEventStore>()
                .As<IEventStore>();

            return builder.Build();
        }
    }
}
