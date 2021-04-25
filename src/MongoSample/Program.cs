using System;
using System.Threading.Tasks;
using Autofac;
using Tangram;
using Tangram.Exceptions;

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

                //var person = new Person(
                //    18,
                //    "Brian",
                //    "Delaney", new
                //        DateTime(2020, 04, 22),
                //    Guid.NewGuid());

                //person.UpdateAge(23, DateTime.UtcNow.AddHours(1), Guid.NewGuid());

                //person.UpdateName("Jimmy", "Delaney", DateTime.UtcNow.AddHours(4), Guid.NewGuid());

                //eventStore.Save<Person, PersonSnapshot>(person);

                //UpdateAge(scope, person.Id);

                //UpdateName(scope, person.Id);

                //UpdateAge(scope, person.Id);

                var id = Guid.Parse("d2c4feb1-913b-4606-a649-38bf6c90ddf4");

                BackgroundUpdate(id);

                NonBackgroundUpdate(id).Wait();

            }

            Console.WriteLine("Finished");
            Console.ReadKey();
        }

        public static async Task BackgroundUpdate(Guid id)
        {
            var container = Bootstrapper.Bootstrap();

            using (var scope = container.BeginLifetimeScope())
            {
                var eventStore = scope.Resolve<IEventStore>();

                try
                {
                    var person = eventStore.GetById<Person, PersonSnapshot>(id);

                    person.UpdateAge(55, DateTime.UtcNow.AddHours(1), Guid.NewGuid());

                    await Task.Delay(TimeSpan.FromSeconds(30));

                    eventStore.Save<Person, PersonSnapshot>(person);
                }
                catch (AggregateConflictException ex)
                {
                    var p = eventStore.GetById<Person, PersonSnapshot>(id);

                    p.UpdateAge(55, DateTime.UtcNow.AddHours(1), Guid.NewGuid());

                    eventStore.Save<Person, PersonSnapshot>(p);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        public static async Task NonBackgroundUpdate(Guid id)
        {
            var container = Bootstrapper.Bootstrap();

            using (var scope = container.BeginLifetimeScope())
            {
                try
                {
                    var eventStore = scope.Resolve<IEventStore>();

                    var person = eventStore.GetById<Person, PersonSnapshot>(id);

                    person.UpdateAge(66, DateTime.UtcNow.AddHours(1), Guid.NewGuid());

                    eventStore.Save<Person, PersonSnapshot>(person);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        private static void UpdateAge(ILifetimeScope scope, Guid id)
        {
            var eventStore = scope.Resolve<IEventStore>();

            var person = eventStore.GetById<Person, PersonSnapshot>(id);

            person.UpdateAge(34, DateTime.UtcNow.AddHours(1), Guid.NewGuid());

            eventStore.Save<Person, PersonSnapshot>(person);
        }

        private static void UpdateName(ILifetimeScope scope, Guid id)
        {
            var eventStore = scope.Resolve<IEventStore>();

            var person = eventStore.GetById<Person, PersonSnapshot>(id);

            person.UpdateName("Jim T", "Delaney", DateTime.UtcNow.AddHours(4), Guid.NewGuid());

            eventStore.Save<Person, PersonSnapshot>(person);
        }

        private Guid CreateNewPerson(ILifetimeScope scope)
        {
            var eventStore = scope.Resolve<IEventStore>();

            var person = new Person(
                18,
                "Brian",
                "Delaney", new
                    DateTime(2020, 04, 22),
                Guid.NewGuid());

            person.UpdateAge(23, DateTime.UtcNow.AddHours(1), Guid.NewGuid());

            person.UpdateName("Jimmy", "Delaney", DateTime.UtcNow.AddHours(4), Guid.NewGuid());

            eventStore.Save<Person, PersonSnapshot>(person);

            return person.Id;
        }
    }
}
