using System;
using Autofac;
using Tangram;

namespace MongoSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = Bootstrapper.Bootstrap();

            using (var scope = container.BeginLifetimeScope())
            {
                var eventStore = scope.Resolve<IEventStore>();

                var person = new Person(
                    18,
                    "Brian",
                    "Delaney", new
                        DateTime(2020, 04, 22),
                    Guid.NewGuid());

                eventStore.Save(person);
            }

            Console.WriteLine("Finished");
            Console.ReadKey();
        }
    }
}
