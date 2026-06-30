using System.Collections.Generic;
using CrudDatastore.Framework;
using CrudDatastore.Samples.SqlClient.Entities;

namespace CrudDatastore.Samples.SqlClient
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
}
