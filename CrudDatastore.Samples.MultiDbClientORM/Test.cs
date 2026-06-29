using System;
using System.Collections.Generic;
using System.Linq;
using CrudDatastore.MultiDbClientORM;
using CrudDatastore.MultiDbClientORM.Entities;
using NUnit.Framework;

namespace CrudDatastore.MultiDbClientORM
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
                    Lastname = "Koch",
                    Identifications = new List<Identification>
                    {
                        new Identification
                        {
                            Type = 1,
                            Number = "222-222-2222"
                        }
                    }
                };

                context.Add(person);
                context.SaveChanges();

                Assert.IsTrue(person.PersonId > 0);
                Assert.IsTrue(context.Find<Person>(p => p.PersonId == person.PersonId).Count() == 1);
                Assert.IsTrue(context.FindSingle<Person>(p => p.PersonId == person.PersonId).Identifications.Count == 1);
            }
        }

        [Test()]
        public void UpdateAction()
        {
            using (var context = DataContext.Factory())
            {
                var person = context.FindSingle<Person>(p => p.PersonId == 1);
                person.Firstname = "Rudolf";
                person.Identifications.Add(
                    new Identification
                    {
                        Type = 1,
                        Number = "333-333"
                    }
                );

                context.Update(person);
                context.SaveChanges();

                Assert.IsTrue(context.FindSingle<Person>(p => p.PersonId == person.PersonId).Firstname == "Rudolf");
                Assert.IsTrue(context.FindSingle<Person>(p => p.PersonId == person.PersonId).Identifications.Count == 3);
            }
        }

        [Test()]
        public void DeleteAction()
        {
            using (var context = DataContext.Factory())
            {
                var person = context.FindSingle<Person>(p => p.PersonId == 1);

                context.Delete(person);
                context.SaveChanges();

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
