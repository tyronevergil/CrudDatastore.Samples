using System;
using System.Linq;
using CrudDatastore.SqlClient;
using CrudDatastore.SqlClient.Entities;
using NUnit.Framework;

namespace CrudDatastore.SqlClient
{
    [TestFixture()]
    public class Test
    {
        [Test()]
        public void CreateAction()
        {
            using (var context = DataContext.Factory())
            {
                var person = new Person
                {
                    Firstname = "Pauline",
                    Lastname = "Koch"
                };

                context.Add(person);

                Assert.IsTrue(person.PersonId > 0);
                Assert.IsTrue(context.Find<Person>(p => p.PersonId == person.PersonId).Count() == 1);
            }
        }

        [Test()]
        public void UpdateAction()
        {
            using (var context = DataContext.Factory())
            {
                var person = context.FindSingle<Person>(p => p.PersonId == 1);
                person.Firstname = "Rudolf";

                context.Update(person);

                Assert.IsTrue(context.FindSingle<Person>(p => p.PersonId == person.PersonId).Firstname == "Rudolf");
            }
        }

        [Test()]
        public void DeleteAction()
        {
            using (var context = DataContext.Factory())
            {
                var person = context.FindSingle<Person>(p => p.PersonId == 1);

                context.Delete(person);

                Assert.IsTrue(context.Find<Person>(p => p.PersonId == person.PersonId).Count() == 0);
            }
        }

        [Test()]
        public void FindAction()
        {
            using (var context = DataContext.Factory())
            {
                var people = context.Find<Person>(p => true);

                Assert.IsTrue(people.Count() == 3);
            }
        }
    }
}
