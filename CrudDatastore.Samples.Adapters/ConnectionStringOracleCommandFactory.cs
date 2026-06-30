using Oracle.ManagedDataAccess.Client;

namespace CrudDatastore.Samples.Adapters.Oracle
{
    public class ConnectionStringOracleCommandFactory : IOracleCommandFactory
    {
        private readonly string _connectionString;

        public ConnectionStringOracleCommandFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public OracleCommand CreateOracleCommand()
        {
            var connection = new OracleConnection(_connectionString);
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
