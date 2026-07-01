using System.Data.SqlClient;
using CrudDatastore.Framework;
using CrudDatastore.Samples.Adapters.Sql;
using CrudDatastore.Samples.SqlClient.Entities;

namespace CrudDatastore.Samples.SqlClient
{
    public class SqlClientUnitOfWork : UnitOfWorkBase, ISqlCommandFactory
    {
        private readonly string _connectionString;

        public SqlClientUnitOfWork(string connectionString)
        {
            _connectionString = connectionString;

            this.Register(new DataStore<Person>(new SqlClientCrudAdapter<Person>(this, "People", p => p.PersonId)));
            this.Register(new DataStore<Identification>(new SqlClientCrudAdapter<Identification>(this, "Identifications", i => i.IdentificationId)));
        }

        public SqlCommand CreateSqlCommand()
        {
            var connection = new SqlConnection(_connectionString);
            return connection.CreateCommand();
        }
    }
}
