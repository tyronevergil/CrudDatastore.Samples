using System;
using System.Data.SqlClient;
using CrudDatastore.SqlClientORM.Entities;

namespace CrudDatastore.SqlClientORM
{
    public class SqlClientUnitOfWork : UnitOfWorkBase, ISqlCommandFactory
    {
        private bool _disposed;

        private string _connectionString;

        private SqlConnection _activeConnection;
        private SqlTransaction _activeTransaction;

        public SqlClientUnitOfWork(string connectionString)
        {
            _connectionString = connectionString;

            var dataStorePerson = new DataStore<Person>(new SqlClientCrudAdapter<Person>(this, "People", p => p.PersonId));
            var dataStoreIdentification = new DataStore<Identification>(new SqlClientCrudAdapter<Identification>(this, "Identifications", p => p.IdentificationId));

            this.Register(dataStorePerson)
                .Map(p => p.Identifications, (p, i) => p.PersonId == i.PersonId);
            this.Register(dataStoreIdentification);
        }

        public SqlCommand CreateSqlCommand()
        {
            if (_activeConnection != null)
            {
                var command = _activeConnection.CreateCommand();
                if (_activeTransaction != null)
                {
                    command.Transaction = _activeTransaction;
                }

                return command;
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
                var transaction = connection.BeginTransaction();

                _activeConnection = connection;
                _activeTransaction = transaction;

                try
                {
                    base.Commit();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    _activeTransaction = null;
                    _activeConnection = null;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects)
                }

                if (_activeTransaction != null)
                {
                    _activeTransaction.Dispose();
                }

                if (_activeConnection != null)
                {
                    _activeConnection.Dispose();
                }

                //
                _disposed = true;
            }

            base.Dispose(disposing);
        }

        ~SqlClientUnitOfWork()
        {
            Dispose(false);
        }
    }

    public interface ISqlCommandFactory
    {
        SqlCommand CreateSqlCommand();
    }
}
