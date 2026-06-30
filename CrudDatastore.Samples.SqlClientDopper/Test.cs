using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using CrudDatastore.Samples.SqlClientDopper.Entities;
using NUnit.Framework;

namespace CrudDatastore.Samples.SqlClientDopper
{
    [TestFixture]
    [Category("Integration")]
    public class Test
    {
        private string _connectionString;

        [SetUp]
        public void SetUp()
        {
            _connectionString = ConfigurationManager.AppSettings["SqlClient.ConnectionString"];
            Assume.That(!string.IsNullOrWhiteSpace(_connectionString),
                "SqlClient.ConnectionString is not configured — skipping integration tests.");

            // Seed baseline data (People table must exist — run Scripts/SqlServer.sql first)
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Add(new Person { Firstname = "Hermann", Lastname = "Einstein" });
                connection.Add(new Person { Firstname = "Albert",  Lastname = "Einstein" });
                connection.Add(new Person { Firstname = "Maja",    Lastname = "Einstein" });
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return;

            using (var connection = new SqlConnection(_connectionString))
            {
                foreach (var p in connection.Find<Person>(p => true).ToList())
                    connection.Delete(p);
            }
        }

        [Test]
        public void CreateAction()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var person = new Person { Firstname = "Pauline", Lastname = "Koch" };

                connection.Add(person);

                Assert.IsTrue(person.PersonId > 0);
                Assert.IsTrue(connection.Find<Person>(p => p.PersonId == person.PersonId).Count() == 1);
            }
        }

        [Test]
        public void UpdateAction()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var person = connection.Find<Person>(p => p.Firstname == "Hermann").First();
                person.Firstname = "Rudolf";

                connection.Update(person);

                Assert.IsTrue(connection.FindSingle<Person>(p => p.PersonId == person.PersonId).Firstname == "Rudolf");
            }
        }

        [Test]
        public void DeleteAction()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var person = connection.Find<Person>(p => p.Firstname == "Hermann").First();

                connection.Delete(person);

                Assert.IsTrue(connection.Find<Person>(p => p.PersonId == person.PersonId).Count() == 0);
            }
        }

        [Test]
        public void FindAction()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var people = connection.Find<Person>(p => true);

                Assert.IsTrue(people.Count() == 3);
            }
        }
    }
}


