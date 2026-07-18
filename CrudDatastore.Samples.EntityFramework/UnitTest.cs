using CrudDatastore.Samples.EntityFramework.Entities;
using CrudDatastore.Samples.EntityFramework.Specifications;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace CrudDatastore.Samples.EntityFramework
{
    [TestFixture]
    public class UnitTest
    {
        [Test]
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
                            Type = Identification.Types.SSN,
                            Number = "222-222-2222"
                        }
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
            using (var context = DataContext.Factory())
            {
                var person = context.FindSingle(PersonSpecs.Get(1));
                person.Firstname = "Rudolf";
                person.Identifications.Add(
                    new Identification
                    {
                        Type = Identification.Types.TIN,
                        Number = "333-333"
                    }
                );

                context.Update(person);
                context.SaveChanges();

                Assert.IsTrue(context.FindSingle(PersonSpecs.Get(person.PersonId)).Firstname == "Rudolf");
                Assert.IsTrue(context.FindSingle(PersonSpecs.Get(person.PersonId)).Identifications.Count == 3);
            }
        }

        [Test]
        public void DeleteAction()
        {
            using (var context = DataContext.Factory())
            {
                var person = context.FindSingle(PersonSpecs.Get(1));

                context.Delete(person);
                context.SaveChanges();

                Assert.IsTrue(context.Find(PersonSpecs.Get(person.PersonId)).Count() == 0);
            }
        }

        [Test]
        public void FindAction()
        {
            using (var context = DataContext.Factory())
            {
                var people = context.Find(PersonSpecs.GetAll());

                Assert.IsTrue(people.Count() == 3);
            }
        }
    }
}
