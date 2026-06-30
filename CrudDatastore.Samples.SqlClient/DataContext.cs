using System;
using System.Linq;
using System.Linq.Expressions;
using CrudDatastore;

namespace CrudDatastore.Samples.SqlClient
{
    public class DataContext : DataContextBase
    {
        private DataContext(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public static DataContext Factory()
        {
            //return new DataContext(new SqlClientUnitOfWork(<connectionstring>));
            return new DataContext(new InMemoryUnitOfWork());
        }
    }

    public static class DataContextExtensions
    {
        public static IQueryable<T> Find<T>(this DataContextBase context, Expression<Func<T, bool>> predicate) where T : EntityBase
        {
            return context.Find(new Specification<T>(predicate));
        }

        public static T FindSingle<T>(this DataContextBase context, Expression<Func<T, bool>> predicate) where T : EntityBase
        {
            return context.FindSingle(new Specification<T>(predicate));
        }
    }
}

