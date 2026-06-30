using System.Configuration;
using System.Linq;
using CrudDatastore.Samples.SqlClient.Entities;
using NUnit.Framework;

namespace CrudDatastore.Samples.SqlClient
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
                context.Add(new Person { Firstname = "Hermann", Lastname = "Einstein" });
                context.Add(new Person { Firstname = "Albert",  Lastname = "Einstein" });
                context.Add(new Person { Firstname = "Maja",    Lastname = "Einstein" });
                context.SaveChanges();
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return;

            // Clean up all rows written during the test
            using (var context = DataContext.Factory(_connectionString))
            {
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
                var person = new Person { Firstname = "Pauline", Lastname = "Koch" };

                context.Add(person);
                context.SaveChanges();

                Assert.IsTrue(person.PersonId > 0);
                Assert.IsTrue(context.Find<Person>(p => p.PersonId == person.PersonId).Count() == 1);
            }
        }

        [Test]
        public void UpdateAction()
        {
            using (var context = DataContext.Factory(_connectionString))
            {
                var person = context.Find<Person>(p => p.Firstname == "Hermann").First();
                person.Firstname = "Rudolf";

                context.Update(person);
                context.SaveChanges();

                Assert.IsTrue(context.FindSingle<Person>(p => p.PersonId == person.PersonId).Firstname == "Rudolf");
            }
        }

        [Test]
        public void DeleteAction()
        {
            using (var context = DataContext.Factory(_connectionString))
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
            using (var context = DataContext.Factory(_connectionString))
            {
                var people = context.Find<Person>(p => true);

                Assert.IsTrue(people.Count() == 3);
            }
        }
    }
}
