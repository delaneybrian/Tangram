using System;

namespace Tangram.Sql
{
    internal class SqlCommandFactory
    {
        public static string MakeCountEvents(string tableName, Guid aggregateId)
        {
            return $"SELECT COUNT(*) FROM {tableName} WHERE aggregateId='{aggregateId}';";
        }

        public static string MakeInsert(string tableName, Guid aggregateId, int version, string type, string payload)
        {
            return $"INSERT INTO {tableName} (aggregateId, version, updatedAtUtc, type, payload)" + 
                   $"VALUES ('{aggregateId}', {version}, '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}', '{type}', '{payload}');";
        }

        public static string MakeRetrieveEvents(string tableName, Guid aggregateId)
        {
            return $"SELECT * FROM {tableName} WHERE aggregateId='{aggregateId}' ORDER BY version ASC;";
        }

        public static string MakeRetrieveEvents(string tableName, Guid aggregateId, int minVersion)
        {
            return $"SELECT * FROM {tableName} WHERE aggregateId='{aggregateId}' AND Version > {minVersion} ORDER BY version ASC;";
        }

        public static string MakeSelectLatestSnaphot(string tableName, Guid aggregateId)
        {
            return
                $"SELECT a.aggregateId, a.version, a.payload " +
                $"FROM {tableName} a " +
                $"INNER JOIN(" +
                    $"SELECT aggregateId, MAX(version) version " +
                    $"FROM {tableName} " +
                    $"WHERE aggregateId = '{aggregateId}' " +
                    $"GROUP BY[aggregateId]) b " +
                $"ON a.aggregateId = b.aggregateId AND a.version = b.version;";
        }
    }
}