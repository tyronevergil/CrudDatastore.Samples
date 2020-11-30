using System;
using System.Linq;
using System.Linq.Expressions;

namespace CrudDatastore.SqlClientORM
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

    public static class DataContextExtentions
    {
        public static IQueryable<T> Find<T>(this DataContextBase context, Expression<Func<T, bool>> predicate) where T : EntityBase
        {
            return context.Find(new Specification<T>(predicate));
        }

        public static T FindSingle<T>(this DataContextBase context, Expression<Func<T, bool>> predicate) where T : EntityBase
        {
            return context.FindSingle(new Specification<T>(predicate));
        }

        public static void Execute(this DataContextBase context, string sql, params object[] parameters)
        {
            context.Execute(new Command(sql, parameters));
        }
    }
}
