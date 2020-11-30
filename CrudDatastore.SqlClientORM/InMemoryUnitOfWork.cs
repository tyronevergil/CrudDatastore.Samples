using System;
using System.Collections.Generic;
using CrudDatastore.SqlClientORM.Entities;

namespace CrudDatastore.SqlClientORM
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

            var dataStorePerson = new DataStore<Person>(new InMemoryCrudAdapter<Person>(people, p => p.PersonId));
            var dataStoreIdentification = new DataStore<Identification>(new InMemoryCrudAdapter<Identification>(identifications, p => p.IdentificationId));

            this.Register(dataStorePerson)
                .Map(p => p.Identifications, (p, i) => p.PersonId == i.PersonId);
            this.Register(dataStoreIdentification);
        }
    }
}
