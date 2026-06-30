using CrudDatastore;

namespace CrudDatastore.Samples.MultiDbClientORM
{
    public class DataContext : DataContextBase
    {
        private DataContext(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public static DataContext Factory()
        {
            return new DataContext(new InMemoryUnitOfWork());
        }

        public static DataContext Factory(string sqlClientConnectionString, string oracleClientConnectionString)
        {
            return new DataContext(new MultiDbClientUnitOfWork(sqlClientConnectionString, oracleClientConnectionString));
        }
    }
}


