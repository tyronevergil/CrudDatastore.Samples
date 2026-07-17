using System;
using System.Data;
using System.Data.SqlClient;
using CrudDatastore.Framework;
using CrudDatastore.Samples.Adapters.Sql;
using CrudDatastore.Samples.SqlClient.Entities;

namespace CrudDatastore.Samples.SqlClient
{
    public class SqlClientUnitOfWork : UnitOfWorkBase, ISqlCommandFactory, IDisposable
    {
        private bool _disposed;
        private readonly string _connectionString;

        private SqlConnection _activeConnection;

        public SqlClientUnitOfWork(string connectionString)
        {
            _connectionString = connectionString;

            this.Register(new DataStore<Person>(new SqlClientCrudAdapter<Person>(this, "People", p => p.PersonId)));
            this.Register(new DataStore<Identification>(new SqlClientCrudAdapter<Identification>(this, "Identifications", i => i.IdentificationId)));
        }

        public SqlCommand CreateSqlCommand()
        {
            if (_activeConnection != null)
            {
                return _activeConnection.CreateCommand();
            }
            else
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

        public override void Commit()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                _activeConnection = connection;

                try
                {
                    base.Commit();
                }
                finally
                {
                    _activeConnection = null;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (_activeConnection != null)
                {
                    _activeConnection.Dispose();
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }

        ~SqlClientUnitOfWork()
        {
            Dispose(false);
        }
    }
}
