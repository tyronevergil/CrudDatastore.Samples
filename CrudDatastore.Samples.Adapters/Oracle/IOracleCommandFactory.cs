using Oracle.ManagedDataAccess.Client;

namespace CrudDatastore.Samples.Adapters.Oracle
{
    public interface IOracleCommandFactory
    {
        OracleCommand CreateOracleCommand();
    }
}
