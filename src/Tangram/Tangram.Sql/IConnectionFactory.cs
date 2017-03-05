using System.Data.SqlClient;

namespace Tangram.Sql
{
    public interface IConnectionFactory
    {
        SqlConnection CreateConnection();
    }
}
