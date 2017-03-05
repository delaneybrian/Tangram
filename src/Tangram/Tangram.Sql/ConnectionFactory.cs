using System.Data.SqlClient;

namespace Tangram.Sql
{
    public class ConnectionFactory : IConnectionFactory
    {
        private readonly ISqlEventStoreConfiguration _config;

        public ConnectionFactory(ISqlEventStoreConfiguration config)
        {
            _config = config;
        }

        public SqlConnection CreateConnection()
        {
            var connection = new SqlConnection(_config.ConnectionString);

            connection.Open();

            return connection;
        }
    }
}
