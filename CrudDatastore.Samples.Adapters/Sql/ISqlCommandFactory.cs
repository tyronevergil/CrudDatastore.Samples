using System.Data.SqlClient;

namespace CrudDatastore.Samples.Adapters.Sql
{
    public interface ISqlCommandFactory
    {
        SqlCommand CreateSqlCommand();
    }
}

