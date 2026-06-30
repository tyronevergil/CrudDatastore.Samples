using Oracle.ManagedDataAccess.Client;

namespace CrudDatastore.Samples.Adapters.Oracle
{
    public class OracleCommandFactory : IOracleCommandFactory
    {
        private readonly string _connectionString;

        public OracleCommandFactory(string connectionString)
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
