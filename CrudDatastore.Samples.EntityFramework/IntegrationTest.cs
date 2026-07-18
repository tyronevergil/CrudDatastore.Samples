using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using CrudDatastore.Samples.EntityFramework.Entities;
using CrudDatastore.Samples.EntityFramework.Specifications;
using NUnit.Framework;

namespace CrudDatastore.Samples.EntityFramework
{
    [TestFixture]
    [Category("Integration")]
    public class IntegrationTest
    {
        private string _connectionString;

        [SetUp]
        public void SetUp()
        {
            _connectionString = ConfigurationManager.AppSettings["SqlClient.ConnectionString"];
            Assume.That(!string.IsNullOrWhiteSpace(_connectionString),
                "SqlClient.ConnectionString is not configured — skipping integration tests.");

            // Seed baseline data
            using (var context = DataContext.Factory(_connectionString))
            {
                context.Add(new Person
                {
                    Firstname = "Hermann", Lastname = "Einstein",
                    Identifications = new List<Identification>
                    {
                        new Identification { Type = Identification.Types.SSN, Number = "509-515-224" },
                        new Identification { Type = Identification.Types.TIN, Number = "92-4267" }
                    }
                });
                context.Add(new Person
                {
                    Firstname = "Albert", Lastname = "Einstein",
                    Identifications = new List<Identification>
                    {
                        new Identification { Type = Identification.Types.SSN, Number = "425-428-336" }
                    }
                });
                context.Add(new Person { Firstname = "Maja", Lastname = "Einstein" });
                context.SaveChanges();
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return;

            using (var context = DataContext.Factory(_connectionString))
            {
                foreach (var i in context.Find<Identification>(i => true).ToList())
                    context.Delete(i);
                foreach (var p in context.Find<Person>(p => true).ToList())
                    context.Delete(p);
                context.SaveChanges();
            }
        }

        [Test]
        public void CreateAction()
        {
            using (var context = DataContext.Factory(_connectionString))
            {
                var person = new Person
                {
                    Firstname = "Pauline",
                    Lastname = "Koch",
                    Identifications = new List<Identification>
                    {
                        new Identification { Type = Identification.Types.SSN, Number = "222-222-2222" }
                    }
                };

                context.Add(person);
                context.SaveChanges();

                Assert.IsTrue(person.PersonId > 0);
                Assert.IsTrue(context.Find(PersonSpecs.Get(person.PersonId)).Count() == 1);
                Assert.IsTrue(context.FindSingle(PersonSpecs.Get(person.PersonId)).Identifications.Count == 1);
            }
        }

        [Test]
        public void UpdateAction()
        {
            using (var context = DataContext.Factory(_connectionString))
            {
                var person = context.Find(PersonSpecs.Get("Hermann")).First();
                person.Firstname = "Rudolf";
                person.Identifications.Add(new Identification { Type = Identification.Types.TIN, Number = "333-333" });

                context.Update(person);
                context.SaveChanges();

                Assert.IsTrue(context.FindSingle(PersonSpecs.Get(person.PersonId)).Firstname == "Rudolf");
                Assert.IsTrue(context.FindSingle(PersonSpecs.Get(person.PersonId)).Identifications.Count == 3);
            }
        }

        [Test]
        public void DeleteAction()
        {
            using (var context = DataContext.Factory(_connectionString))
            {
                var person = context.Find(PersonSpecs.Get("Hermann")).First();

                context.Delete(person);
                context.SaveChanges();

                Assert.IsTrue(context.Find(PersonSpecs.Get(person.PersonId)).Count() == 0);
            }
        }

        [Test]
        public void FindAction()
        {
            using (var context = DataContext.Factory(_connectionString))
            {
                var people = context.Find(PersonSpecs.GetAll());

                Assert.IsTrue(people.Count() == 3);
            }
        }
    }
}
