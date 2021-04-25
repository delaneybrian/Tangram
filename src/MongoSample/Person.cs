using System;
using System.Collections.Generic;
using MongoSample.Events;
using Tangram;

namespace MongoSample
{
    public class Person : AggregateBase, ISnapshotAggregate<PersonSnapshot>
    {
        public Person()
        {
            RegisterHandler<CreatePerson>(Apply);
            RegisterHandler<UpdateAge>(Apply);
            RegisterHandler<UpdateName>(Apply);
        }

        public Person(int age, string firstName, string lastName, DateTime createdAtUtc, Guid createdByUserId)
            : this()
        {
            RaiseEvent(new CreatePerson
            {
                AggregateId = Guid.NewGuid(),
                Age = age,
                FirstName = firstName,
                LastName = lastName,
                CreatedAtUtc = createdAtUtc,
                CreatedByUserId = createdByUserId
            });
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Age { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime LastUpdatedAtUtc { get; set; }

        public Guid CreatedByUserId { get; set; }

        public ICollection<Guid> UpdatedByUserIds { get; set; } = new List<Guid>();

        public void RestoreFromSnapshot(PersonSnapshot snapshot)
        {
            Age = snapshot.Age;
            Id = snapshot.AggregateId;
            CreatedAtUtc = snapshot.CreatedAtUtc;
            CreatedByUserId = snapshot.CreatedByUserId;
            FirstName = snapshot.FirstName;
            LastName = snapshot.LastName;
            LastUpdatedAtUtc = snapshot.LastUpdatedAtUtc;
            UpdatedByUserIds = snapshot.UpdatedByUserIds;
            Version = snapshot.Version;
        }

        public PersonSnapshot ToSnapshot()
        {
            return new PersonSnapshot
            {
                Age = Age,
                AggregateId = Id,
                CreatedAtUtc = CreatedAtUtc,
                CreatedByUserId = CreatedByUserId,
                FirstName = FirstName,
                LastName = LastName,
                LastUpdatedAtUtc = LastUpdatedAtUtc,
                UpdatedByUserIds = UpdatedByUserIds,
                Version = Version
            };
        }

        public void UpdateAge(int newAge, DateTime updatedAtUtc, Guid updatedByUserId)
        {
            RaiseEvent(new UpdateAge
            {
                Age = newAge,
                AggregateId = Id,
                UpdatedAtUtc = updatedAtUtc,
                UpdatedByUserId = updatedByUserId
            });
        }

        public void UpdateName(string newFirstName, string newLastName, DateTime updatedAtUtc, Guid updatedByUserId)
        {
            RaiseEvent(new UpdateName
            {
                FirstName = newFirstName,
                LastName = newLastName,
                AggregateId = Id,
                UpdatedAtUtc = updatedAtUtc,
                UpdatedByUserId = updatedByUserId
            });
        }

        private void Apply(CreatePerson evt)
        {
            Id = evt.AggregateId;
            FirstName = evt.FirstName;
            LastName = evt.LastName;
            Age = evt.Age;
            CreatedAtUtc = evt.CreatedAtUtc;
            CreatedByUserId = evt.CreatedByUserId;
            LastUpdatedAtUtc = evt.CreatedAtUtc;
        }

        private void Apply(UpdateAge evt)
        {
            Age = evt.Age;
            LastUpdatedAtUtc = evt.UpdatedAtUtc;
            UpdatedByUserIds.Add(evt.UpdatedByUserId);
        }

        private void Apply(UpdateName evt)
        {
            FirstName = evt.FirstName;
            LastName = evt.LastName;
            LastUpdatedAtUtc = evt.UpdatedAtUtc;
            UpdatedByUserIds.Add(evt.UpdatedByUserId);
        }
    }
}
