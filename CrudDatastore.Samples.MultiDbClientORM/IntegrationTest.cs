using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using CrudDatastore.Samples.MultiDbClientORM.Entities;
using NUnit.Framework;

namespace CrudDatastore.Samples.MultiDbClientORM
{
    [TestFixture]
    [Category("Integration")]
    public class IntegrationTest
    {
        private string _sqlConnectionString;
        private string _oracleConnectionString;

        [SetUp]
        public void SetUp()
        {
            _sqlConnectionString    = ConfigurationManager.AppSettings["SqlClient.ConnectionString"];
            _oracleConnectionString = ConfigurationManager.AppSettings["OracleClient.ConnectionString"];

            Assume.That(!string.IsNullOrWhiteSpace(_sqlConnectionString),
                "SqlClient.ConnectionString is not configured — skipping integration tests.");
            Assume.That(!string.IsNullOrWhiteSpace(_oracleConnectionString),
                "OracleClient.ConnectionString is not configured — skipping integration tests.");

            // Seed baseline data
            using (var context = DataContext.Factory(_sqlConnectionString, _oracleConnectionString))
            {
                context.Add(new Person
                {
                    Firstname = "Hermann", Lastname = "Einstein",
                    Identifications = new List<Identification>
                    {
                        new Identification { Type = 1, Number = "509-515-224" },
                        new Identification { Type = 2, Number = "92-4267" }
                    }
                });
                context.Add(new Person
                {
                    Firstname = "Albert", Lastname = "Einstein",
                    Identifications = new List<Identification>
                    {
                        new Identification { Type = 1, Number = "425-428-336" }
                    }
                });
                context.Add(new Person { Firstname = "Maja", Lastname = "Einstein" });
                context.SaveChanges();
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (string.IsNullOrWhiteSpace(_sqlConnectionString) || string.IsNullOrWhiteSpace(_oracleConnectionString))
                return;

            using (var context = DataContext.Factory(_sqlConnectionString, _oracleConnectionString))
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
            using (var context = DataContext.Factory(_sqlConnectionString, _oracleConnectionString))
            {
                var person = new Person
                {
                    Firstname = "Pauline",
                    Lastname = "Koch",
                    Identifications = new List<Identification>
                    {
                        new Identification { Type = 1, Number = "222-222-2222" }
                    }
                };

                context.Add(person);
                context.SaveChanges();

                Assert.IsTrue(person.PersonId > 0);
                Assert.IsTrue(context.Find<Person>(p => p.PersonId == person.PersonId).Count() == 1);
                Assert.IsTrue(context.FindSingle<Person>(p => p.PersonId == person.PersonId).Identifications.Count == 1);
            }
        }

        [Test]
        public void UpdateAction()
        {
            using (var context = DataContext.Factory(_sqlConnectionString, _oracleConnectionString))
            {
                var person = context.Find<Person>(p => p.Firstname == "Hermann").First();
                person.Firstname = "Rudolf";
                person.Identifications.Add(new Identification { Type = 2, Number = "333-333" });

                context.Update(person);
                context.SaveChanges();

                Assert.IsTrue(context.FindSingle<Person>(p => p.PersonId == person.PersonId).Firstname == "Rudolf");
                Assert.IsTrue(context.FindSingle<Person>(p => p.PersonId == person.PersonId).Identifications.Count == 3);
            }
        }

        [Test]
        public void DeleteAction()
        {
            using (var context = DataContext.Factory(_sqlConnectionString, _oracleConnectionString))
            {
                var person = context.Find<Person>(p => p.Firstname == "Hermann").First();

                context.Delete(person);
                context.SaveChanges();

                Assert.IsTrue(context.Find<Person>(p => p.PersonId == person.PersonId).Count() == 0);
            }
        }

        [Test]
        public void FindAction()
        {
            using (var context = DataContext.Factory(_sqlConnectionString, _oracleConnectionString))
            {
                var people = context.Find<Person>(p => true);

                Assert.IsTrue(people.Count() == 3);
            }
        }
    }
}
