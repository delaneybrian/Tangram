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

                person.UpdateAge(23, DateTime.UtcNow.AddHours(1), Guid.NewGuid());

                person.UpdateName("Jimmy", "Delaney", DateTime.UtcNow.AddHours(4), Guid.NewGuid());

                eventStore.Save<Person, PersonSnapshot>(person);

                UpdateAge(scope, person.Id);

                UpdateName(scope, person.Id);

                UpdateAge(scope, person.Id);
            }

            Console.WriteLine("Finished");
            Console.ReadKey();
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
