using System;
using System.Collections.Generic;
using CrudDatastore;
using CrudDatastore.Framework;
using CrudDatastore.SqlClient.Entities;

namespace CrudDatastore.SqlClient
{
    public class InMemoryUnitOfWork : UnitOfWorkBase
    {
        public InMemoryUnitOfWork()
        {
            var people = new List<Person>
            {
                new Person { PersonId = 1, Firstname = "Hermann", Lastname = "Einstein "},
                new Person { PersonId = 2, Firstname = "Albert", Lastname = "Einstein "},
                new Person { PersonId = 3, Firstname = "Maja", Lastname = "Einstein "}
            };

            var identifications = new List<Identification>
            {
                new Identification { IdentificationId = 1, PersonId = 1, Type = Identification.Types.SSN, Number = "509–515-224" },
                new Identification { IdentificationId = 2, PersonId = 1, Type = Identification.Types.TIN, Number = "92–4267" },
                new Identification { IdentificationId = 3, PersonId = 2, Type = Identification.Types.SSN, Number = "425–428-336" },
            };

            this.Register(people.CreateDataStore(p => p.PersonId));
            this.Register(identifications.CreateDataStore(p => p.IdentificationId));
        }
    }

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
