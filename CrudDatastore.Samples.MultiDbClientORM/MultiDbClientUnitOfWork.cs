using System.Data.SqlClient;
using System.Transactions;
using CrudDatastore.Framework;
using CrudDatastore.Samples.MultiDbClientORM.Entities;
using CrudDatastore.Samples.Adapters.Oracle;
using CrudDatastore.Samples.Adapters.Sql;
using Oracle.ManagedDataAccess.Client;

namespace CrudDatastore.Samples.MultiDbClientORM
{
    public class MultiDbClientUnitOfWork : UnitOfWorkBase, ISqlCommandFactory, IOracleCommandFactory
    {
        private readonly string _sqlClientConnectionString;
        private readonly string _oracleClientConnectionString;

        public MultiDbClientUnitOfWork(string sqlClientConnectionString, string oracleClientConnectionString)
        {
            _sqlClientConnectionString = sqlClientConnectionString;
            _oracleClientConnectionString = oracleClientConnectionString;

            var dataStorePerson = new DataStore<Person>(new SqlClientCrudAdapter<Person>(this, "People", p => p.PersonId));
            var dataStoreIdentification = new DataStore<Identification>(new OracleClientCrudAdapter<Identification>(this, "Identifications", p => p.IdentificationId));

            this.Register(dataStorePerson)
                .Map(p => p.Identifications, (p, i) => p.PersonId == i.PersonId);
            this.Register(dataStoreIdentification);
        }

        public SqlCommand CreateSqlCommand()
        {
            var connection = new SqlConnection(_sqlClientConnectionString);
            return connection.CreateCommand();
        }

        public OracleCommand CreateOracleCommand()
        {
            var connection = new OracleConnection(_oracleClientConnectionString);
            return connection.CreateCommand();
        }

        public override void Commit()
        {
            // Sequential commit: SQL Server first, then Oracle.
            // Each database commits in its own local transaction (managed inside each adapter).
            // This is best-effort — if the Oracle commit fails after SQL Server has already
            // committed, the two databases will be inconsistent. For a samples project this
            // trade-off is acceptable; for production see the TransactionScope note below.
            base.Commit();

            // -----------------------------------------------------------------------
            // OPTION: Distributed transaction via TransactionScope + MSDTC
            // -----------------------------------------------------------------------
            // Replace the base.Commit() call above with the block below to get true
            // two-phase commit across SQL Server and Oracle.
            //
            // Requirements:
            //   1. Windows host — MSDTC is a Windows-only service (not available in
            //      Linux Docker containers).
            //   2. SQL Server on Windows with the Distributed Transaction Coordinator
            //      service running and network DTC access enabled.
            //   3. Oracle with the Oracle Services for Microsoft Transaction Server (OraMTS)
            //      component installed and configured on the same Windows host.
            //   4. Both connection strings must point to Windows-hosted instances, not
            //      the Linux Docker containers supplied with this repo.
            //
            // Once the above are in place, swap the sequential commit for:
            //
            // using (var scope = new TransactionScope())
            // {
            //     base.Commit();   // enlists both connections; escalates to MSDTC
            //     scope.Complete();
            // }
            // -----------------------------------------------------------------------
        }
    }
}

