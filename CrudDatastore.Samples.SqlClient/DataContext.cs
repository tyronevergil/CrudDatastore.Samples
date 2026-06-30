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
            return new DataContext(new InMemoryUnitOfWork());
        }

        public static DataContext Factory(string connectionString)
        {
            return new DataContext(new SqlClientUnitOfWork(connectionString));
        }
    }
}


