using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CrudDatastore;
using CrudDatastore.SqlClient.Entities;

namespace CrudDatastore.SqlClient
{
    public interface IDataStoreConfig : IDataCommand
    {
        void RegisterDataStores(IDataStoreRegistry registry);
    }

    public class InMemoryDataStoreConfig : IDataStoreConfig
    {
        private readonly IList<Person> _people;
        private readonly IList<Identification> _identifications;

        public InMemoryDataStoreConfig()
        {
            _people = new List<Person>
            {
                new Person { PersonId = 1, Firstname = "Hermann", Lastname = "Einstein "},
                new Person { PersonId = 2, Firstname = "Albert", Lastname = "Einstein "},
                new Person { PersonId = 3, Firstname = "Maja", Lastname = "Einstein "}
            };

            _identifications = new List<Identification>
            {
                new Identification { IdentificationId = 1, PersonId = 1, Type = Identification.Types.SSN, Number = "509–515-224" },
                new Identification { IdentificationId = 2, PersonId = 1, Type = Identification.Types.TIN, Number = "92–4267" },
                new Identification { IdentificationId = 3, PersonId = 2, Type = Identification.Types.SSN, Number = "425–428-336" },
            };
        }

        public void RegisterDataStores(IDataStoreRegistry registry)
        {           
            registry.Register(new DataStore<Person>(new InMemoryCrudAdapter<Person>(_people, p => p.PersonId)));
            registry.Register(new DataStore<Identification>(new InMemoryCrudAdapter<Identification>(_identifications, p => p.IdentificationId)));
        }

        void IDataCommand.Execute(string command, params object[] parameters)
        {

        }
    }

    public class SqlClientDataStoreConfig : IDataStoreConfig
    {
        private string _connectionString;

        public SqlClientDataStoreConfig(string connectString)
        {
            _connectionString = connectString;
        }

        public void RegisterDataStores(IDataStoreRegistry registry)
        {
            registry.Register(new DataStore<Person>(new SqlClientCrudAdapter<Person>(_connectionString, "People", p => p.PersonId)));
            registry.Register(new DataStore<Identification>(new SqlClientCrudAdapter<Identification>(_connectionString, "Identifications", i => i.IdentificationId)));
        }

        void IDataCommand.Execute(string command, params object[] parameters)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand(command, con))
                {
                    var i = 0;
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(string.Format("@{0}", i), param ?? DBNull.Value);
                        i++;
                    }

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
