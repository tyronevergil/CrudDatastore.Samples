using System;
using System.Linq;
using System.Linq.Expressions;
using CrudDatastore;

namespace CrudDatastore.SqlClient
{
    public class DataContext : DataStoreContextBase
    {
        private readonly IDataStoreConfig _dataStoreConfig;

        private DataContext(IDataStoreConfig dataStores)
        {
            _dataStoreConfig = dataStores;
            _dataStoreConfig.RegisterDataStores(this);
        }

        public static DataContext Factory()
        {
            //return new DataContext(new SqlClientDataStoreConfig(<connectionstring>));
            return new DataContext(new InMemoryDataStoreConfig());
        }

        public override void Execute(ICommand command)
        {
            command.SatisfyingFrom(_dataStoreConfig);
        }
    }

    public static class DataQueryExtention
    {
        public static IQueryable<T> Find<T>(this DataQuery<T> query, Expression<Func<T, bool>> predicate) where T : EntityBase
        {
            return query.Find(new Specification<T>(predicate));
        }

        public static T FindSingle<T>(this DataQuery<T> query, Expression<Func<T, bool>> predicate) where T : EntityBase
        {
            return query.FindSingle(new Specification<T>(predicate));
        }
    }

    public static class DataQueryContextExtention
    {
        public static IQueryable<T> Find<T>(this DataQueryContextBase context, Expression<Func<T, bool>> predicate) where T : EntityBase
        {
            return context.Find(new Specification<T>(predicate));
        }

        public static T FindSingle<T>(this DataQueryContextBase context, Expression<Func<T, bool>> predicate) where T : EntityBase
        {
            return context.FindSingle(new Specification<T>(predicate));
        }

        public static void Execute(this DataQueryContextBase context, string sql, params object[] parameters)
        {
            context.Execute(new Command(sql, parameters));
        }
    }
}
