using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Tangram.Exceptions;

namespace Tangram.Sql
{
    public class SqlEventStore : EventStoreBase, IEventStore
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly ISqlEventStoreConfiguration _sqlEventStoreConfig;
        private readonly ISerializer _serializer;
        private readonly int _snapshotFrequency;

        public SqlEventStore(IConnectionFactory connectionFactory, ISqlEventStoreConfiguration sqlEventStoreConfig, ISerializer serializer)
            : this(connectionFactory, sqlEventStoreConfig, serializer, -1)
        {
        }

        public SqlEventStore(IConnectionFactory connectionFactory, ISqlEventStoreConfiguration sqlEventStoreConfig, ISerializer serializer, int snapshotFrequency)
        {
            _connectionFactory = connectionFactory;
            _sqlEventStoreConfig = sqlEventStoreConfig;
            _serializer = serializer;
            _snapshotFrequency = snapshotFrequency;
        }

        public override IEnumerable<IEvent> Save<TAggregate>(TAggregate aggregate)
        {
            using (var db = _connectionFactory.CreateConnection())
            {
                return SaveEvents(db, aggregate);
            }
        }

        public override IEnumerable<IEvent> Save<TAggregate, TSnapshot>(TAggregate aggregate)
        {
            using (var db = _connectionFactory.CreateConnection())
            {
                var evts = SaveEvents(db, aggregate).ToList();

                if (SnapshotRequired(evts, _snapshotFrequency))
                {
                    var snapshot = aggregate.ToSnapshot();
                    var json = _serializer.Serialize(snapshot);

                    var insertSql = SqlCommandFactory.MakeInsert(_sqlEventStoreConfig.SnapshotsTableName, aggregate.Id,
                        snapshot.Version, snapshot.GetType().AssemblyQualifiedName, json);

                    var insert = new SqlCommand(insertSql, db);

                    insert.ExecuteNonQuery();
                }

                return evts;
            }
        }

        public override TAggregate GetById<TAggregate>(Guid id)
        {
            using (var db = _connectionFactory.CreateConnection())
            {
                return (TAggregate)GetById(db, id, typeof(TAggregate), () => new TAggregate());
            }
        }

        public override TAggregate GetById<TAggregate, TSnapshot>(Guid id)
        {
            using (var db = _connectionFactory.CreateConnection())
            {
                Func<IAggregate> makeAggregate = () =>
                {
                    var aggregate = new TAggregate();

                    if (_snapshotFrequency <= 0)
                        return aggregate;

                    var sql = SqlCommandFactory.MakeSelectLatestSnaphot(_sqlEventStoreConfig.SnapshotsTableName, id);

                    var cmd = new SqlCommand(sql, db);

                    using (var dr = cmd.ExecuteReader())
                    {
                        if (!dr.HasRows)
                        {
                            return aggregate;
                        }

                        while (dr.Read())
                        {
                            var snapshot = (TSnapshot) _serializer.Deserialize<IAggregateSnapshot>(dr["payload"].ToString());

                            aggregate.RestoreFromSnapshot(snapshot);
                        }
                    }

                    return aggregate;
                };

                return (TAggregate)GetById(db, id, typeof(TAggregate), makeAggregate);
            }
        }

        private IEnumerable<IEvent> SaveEvents(SqlConnection db, IAggregate aggregate)
        {
            if (!aggregate.UncommitedEvents().Any())
            {
                return new List<IEvent>();
            }

            var evtsToSave = aggregate.UncommitedEvents().ToList();
            var expected = CalculateExpectedVersion(aggregate, evtsToSave);

            var transaction = db.BeginTransaction(IsolationLevel.ReadCommitted);

            try
            {
                var sql = SqlCommandFactory.MakeCountEvents(_sqlEventStoreConfig.EventsTableName, aggregate.Id);

                var getEvents = new SqlCommand(sql, db, transaction);

                var current = (int) getEvents.ExecuteScalar() - 1;

                if (expected != current)
                {
                    throw new AggregateConflictException(aggregate.Id, expected, current);
                }

                var insertSql = new StringBuilder();

                foreach (var e in evtsToSave)
                {
                    var version = e.Version;
                    var type = e.GetType().AssemblyQualifiedName;

                    var payload = _serializer.Serialize(e).Replace("'", "''");

                    var insertCmd = SqlCommandFactory.MakeInsert(_sqlEventStoreConfig.EventsTableName, aggregate.Id,
                        version, type, payload);

                    insertSql.AppendLine(insertCmd);
                }

                var insert = new SqlCommand(insertSql.ToString(), db, transaction);

                try
                {
                    if (insert.ExecuteNonQuery() != evtsToSave.Count)
                    {
                        throw new AggregateConflictException(aggregate.Id, expected, current);
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("duplicate"))
                        throw new AggregateConflictException(aggregate.Id, expected, current);

                    throw;
                }

                transaction.Commit();
            }
            catch (Exception)
            {
                if (transaction.Connection != null)
                    transaction.Rollback();

                throw;
            }

            return aggregate.UncommitedEvents();
        }

        private IAggregate GetById(SqlConnection db, Guid id, Type type, Func<IAggregate> aggregateFunc)
        {
            var aggregate = aggregateFunc();

            var sql = SqlCommandFactory.MakeRetrieveEvents(_sqlEventStoreConfig.EventsTableName, id, aggregate.Version);

            var cmd = new SqlCommand(sql, db);

            using (var dr = cmd.ExecuteReader())
            {
                if (!dr.HasRows && aggregate.Version == -1)
                {
                    throw new AggregateNotFoundException(type, id);
                }

                while (dr.Read())
                {
                    var evt = _serializer.Deserialize<IEvent>(dr["payload"].ToString());

                    aggregate.ApplyEvent(evt);
                }
            }

            return aggregate;
        }

    }
}

