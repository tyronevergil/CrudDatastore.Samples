using System.Data.SqlClient;

namespace CrudDatastore.Samples.Adapters.Sql
{
    public class SqlCommandFactory : ISqlCommandFactory
    {
        private readonly string _connectionString;

        public SqlCommandFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SqlCommand CreateSqlCommand()
        {
            var connection = new SqlConnection(_connectionString);
            var command = connection.CreateCommand();
            command.Disposed += (sender, e) =>
            {
                connection.Close();
                connection.Dispose();
            };

            connection.Open();
            return command;
        }
    }
}
