using CrudDatastore;
using CrudDatastore.Framework;
using CrudDatastore.Samples.Adapters.Sql;
using CrudDatastore.Samples.SqlClient.Entities;

namespace CrudDatastore.Samples.SqlClient
{
    public class SqlClientUnitOfWork : UnitOfWorkBase
    {
        private readonly string _connectionString;

        public SqlClientUnitOfWork(string connectionString)
        {
            _connectionString = connectionString;

            this.Register(new DataStore<Person>(new SqlClientCrudAdapter<Person>(_connectionString, "People", p => p.PersonId)));
            this.Register(new DataStore<Identification>(new SqlClientCrudAdapter<Identification>(_connectionString, "Identifications", i => i.IdentificationId)));
        }
    }
}
