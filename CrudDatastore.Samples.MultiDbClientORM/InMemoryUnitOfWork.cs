using System;
using System.Collections.Generic;
using CrudDatastore;
using CrudDatastore.Framework;
using CrudDatastore.Samples.MultiDbClientORM.Entities;

namespace CrudDatastore.Samples.MultiDbClientORM
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
                new Identification { IdentificationId = 1, PersonId = 1, Type = 1, Number = "509–515-224" },
                new Identification { IdentificationId = 2, PersonId = 1, Type = 2, Number = "92–4267" },
                new Identification { IdentificationId = 3, PersonId = 2, Type = 1, Number = "425–428-336" },
            };

            var dataStorePerson = people.CreateDataStore(p => p.PersonId);
            var dataStoreIdentification = identifications.CreateDataStore(p => p.IdentificationId);

            this.Register(dataStorePerson)
                .Map(p => p.Identifications, (p, i) => p.PersonId == i.PersonId);
            this.Register(dataStoreIdentification);
        }
    }
}

