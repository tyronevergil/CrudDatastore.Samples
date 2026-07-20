using System;
using System.Linq;
using System.Linq.Expressions;
using System.Configuration;
using CrudDatastore;

namespace CrudDatastore.Samples.EntityFramework
{
    public class DataContext : DataContextBase
    {
        private DataContext(IUnitOfWorkSync unitOfWorkSync)
            : base(unitOfWorkSync)
        {
        }

        private DataContext(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public static DataContext Factory()
        {
            return new DataContext(new InMemoryUnitOfWork());
        }

        public static DataContext Factory(string connectionString)
        {
            return new DataContext(new EFUnitOfWork(connectionString));
        }
    }
}
