using System;
using System.Data;
using System.Data.Linq.Mapping;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace CrudDatastore.SqlClientDopper
{
    public static class SqlClientExtensions
    {
        public static IQueryable<T> Find<T>(this SqlConnection connection, Expression<Func<T, bool>> predicate) where T : EntityBase, new()
        {
            return Find(connection, GetTableName<T>(), predicate);
        }

        public static IQueryable<T> Find<T>(this SqlConnection connection, string tableName, Expression<Func<T, bool>> predicate) where T : EntityBase, new()
        {
            return Find(connection, tableName, new Specification<T>(predicate));
        }

        public static IQueryable<T> Find<T>(this SqlConnection connection, Specification<T> specification) where T : EntityBase, new()
        {
            return Find(connection, GetTableName<T>(), specification);
        }

        public static IQueryable<T> Find<T>(this SqlConnection connection, string tableName, Specification<T> specification) where T : EntityBase, new()
        {
            var factory = new CommandFactory(() => connection.CreateCommand());

            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            var dataQuery = new DataQuery<T>(new SqlClientCrudAdapter<T>(factory, tableName));
            return dataQuery.Find(specification);
        }

        public static T FindSingle<T>(this SqlConnection connection, Expression<Func<T, bool>> predicate) where T : EntityBase, new()
        {
            return FindSingle(connection, GetTableName<T>(), predicate);
        }

        public static T FindSingle<T>(this SqlConnection connection, string tableName, Expression<Func<T, bool>> predicate) where T : EntityBase, new()
        {
            return FindSingle(connection, tableName, new Specification<T>(predicate));
        }

        public static T FindSingle<T>(this SqlConnection connection, Specification<T> specification) where T : EntityBase, new()
        {
            return FindSingle(connection, GetTableName<T>(), specification);
        }

        public static T FindSingle<T>(this SqlConnection connection, string tableName, Specification<T> specification) where T : EntityBase, new()
        {
            var factory = new CommandFactory(() => connection.CreateCommand());
            
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            var dataQuery = new DataQuery<T>(new SqlClientCrudAdapter<T>(factory, tableName));
            return dataQuery.FindSingle(specification);
        }

        public static void Add<T>(this SqlConnection connection, T entity) where T : EntityBase, new()
        {
            Add(connection, GetTableName<T>(), entity);
        }

        public static void Add<T>(this SqlConnection connection, T entity, SqlTransaction transaction) where T : EntityBase, new()
        {
            Add(connection, GetTableName<T>(), entity, transaction);
        }

        public static void Add<T>(this SqlConnection connection, string tableName, T entity) where T : EntityBase, new()
        {
            Add(connection, tableName, GetPropertyKey<T>(), entity);
        }

        public static void Add<T>(this SqlConnection connection, string tableName, T entity, SqlTransaction transaction) where T : EntityBase, new()
        {
            Add(connection, tableName, GetPropertyKey<T>(), entity, transaction);
        }

        public static void Add<T>(this SqlConnection connection, string tableName, Expression<Func<T, object>> key, T entity) where T : EntityBase, new()
        {
            Add(connection, tableName, key, IsIdentity<T>(GetPropertyKeyName<T>(key)), entity);
        }

        public static void Add<T>(this SqlConnection connection, string tableName, Expression<Func<T, object>> key, T entity, SqlTransaction transaction) where T : EntityBase, new()
        {
            Add(connection, tableName, key, IsIdentity<T>(GetPropertyKeyName<T>(key)), entity, transaction);
        }

        public static void Add<T>(this SqlConnection connection, string tableName, Expression<Func<T, object>> key, bool isIdentity, T entity) where T : EntityBase, new()
        {
            Add(connection, tableName, key, isIdentity, entity, null);
        }

        public static void Add<T>(this SqlConnection connection, string tableName, Expression<Func<T, object>> key, bool isIdentity, T entity, SqlTransaction transaction) where T : EntityBase, new()
        {
            var factory = new CommandFactory(() =>
            {
                var sqlCommand = connection.CreateCommand();
                if (transaction != null)
                {
                    sqlCommand.Transaction = transaction;
                }

                return sqlCommand;
            });

            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            var dataStore = new DataStore<T>(new SqlClientCrudAdapter<T>(factory, tableName, key, isIdentity));
            dataStore.Add(entity);
        }

        public static void Update<T>(this SqlConnection connection, T entity) where T : EntityBase, new()
        {
            Update(connection, GetTableName<T>(), entity);
        }

        public static void Update<T>(this SqlConnection connection, T entity, SqlTransaction transaction) where T : EntityBase, new()
        {
            Update(connection, GetTableName<T>(), entity, transaction);
        }

        public static void Update<T>(this SqlConnection connection, string tableName, T entity) where T : EntityBase, new()
        {
            Update(connection, tableName, GetPropertyKey<T>(), entity);
        }

        public static void Update<T>(this SqlConnection connection, string tableName, T entity, SqlTransaction transaction) where T : EntityBase, new()
        {
            Update(connection, tableName, GetPropertyKey<T>(), entity, transaction);
        }

        public static void Update<T>(this SqlConnection connection, string tableName, Expression<Func<T, object>> key, T entity) where T : EntityBase, new()
        {
            Update(connection, tableName, key, entity, null);
        }

        public static void Update<T>(this SqlConnection connection, string tableName, Expression<Func<T, object>> key, T entity, SqlTransaction transaction) where T : EntityBase, new()
        {
            var factory = new CommandFactory(() =>
            {
                var sqlCommand = connection.CreateCommand();
                if (transaction != null)
                {
                    sqlCommand.Transaction = transaction;
                }

                return sqlCommand;
            });

            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            var dataStore = new DataStore<T>(new SqlClientCrudAdapter<T>(factory, tableName, key));
            dataStore.Update(entity);
        }

        public static void Delete<T>(this SqlConnection connection, T entity) where T : EntityBase, new()
        {
            Delete(connection, GetTableName<T>(), entity);
        }

        public static void Delete<T>(this SqlConnection connection, T entity, SqlTransaction transaction) where T : EntityBase, new()
        {
            Delete(connection, GetTableName<T>(), entity, transaction);
        }

        public static void Delete<T>(this SqlConnection connection, string tableName, T entity) where T : EntityBase, new()
        {
            Delete(connection, tableName, GetPropertyKey<T>(), entity);
        }

        public static void Delete<T>(this SqlConnection connection, string tableName, T entity, SqlTransaction transaction) where T : EntityBase, new()
        {
            Delete(connection, tableName, GetPropertyKey<T>(), entity, transaction);
        }

        public static void Delete<T>(this SqlConnection connection, string tableName, Expression<Func<T, object>> key, T entity) where T : EntityBase, new()
        {
            Delete(connection, tableName, key, entity, null);
        }

        public static void Delete<T>(this SqlConnection connection, string tableName, Expression<Func<T, object>> key, T entity, SqlTransaction transaction) where T : EntityBase, new()
        {
            var factory = new CommandFactory(() =>
            {
                var sqlCommand = connection.CreateCommand();
                if (transaction != null)
                {
                    sqlCommand.Transaction = transaction;
                }

                return sqlCommand;
            });

            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            var dataStore = new DataStore<T>(new SqlClientCrudAdapter<T>(factory, tableName, key));
            dataStore.Delete(entity);
        }

        public static void Execute(this SqlConnection connection, string command, params object[] parameters)
        {
            var last = parameters.LastOrDefault();
            if (last != null && last is SqlTransaction)
            {
                var transaction = (SqlTransaction)last;
                parameters = parameters.Where((item, index) => index != parameters.Length - 1).ToArray();

                Execute(connection, new Command(command, parameters), transaction);
            }
            else
            {
                Execute(connection, new Command(command, parameters));
            }
        }

        public static void Execute(this SqlConnection connection, Command command)
        {
            Execute(connection, command, null);
        }

        public static void Execute(this SqlConnection connection, Command command, SqlTransaction transaction)
        {
            var executor = new CommandExecutor((sql, parameters) =>
            {
                using(var sqlCommand = connection.CreateCommand())
                {
                    sqlCommand.CommandText = sql;
                    sqlCommand.CommandType = CommandType.Text;

                    var i = 0;
                    foreach (var param in parameters)
                    {
                        sqlCommand.Parameters.AddWithValue(string.Format("@{0}", i), param ?? DBNull.Value);
                        i++;
                    }

                    if (transaction != null)
                    {
                        sqlCommand.Transaction = transaction;
                    }

                    sqlCommand.ExecuteNonQuery();
                }
            });

            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            command.SatisfyingFrom(executor);
        }

        private static string GetTableName<T>()
        {
            var type = typeof(T);
            var attributes = type.GetCustomAttributes(typeof(TableAttribute), false);
            if (attributes.Any())
            {
                return ((TableAttribute)attributes.First()).Name;
            }

            return type.Name;
        }

        private static Expression<Func<T, object>> GetPropertyKey<T>()
        {
            var possibleKeys = new[] { "Id", string.Format("{0}Id", GetTableName<T>()) };
            var keys = typeof(T).GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(ColumnAttribute), false).Any(c => ((ColumnAttribute)c).IsPrimaryKey) ||
                    possibleKeys.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
                .Select(p => p.Name)
                .ToList();

            if (keys.Any())
            {
                var param = Expression.Parameter(typeof(T));
                var field = Expression.Convert(Expression.PropertyOrField(param, keys.First()), typeof(object));
                return Expression.Lambda<Func<T, object>>(field, param);
            }
            else
            {
                throw new ArgumentException("No key property.");
            }
        }

        private static string GetPropertyKeyName<T>(Expression<Func<T, object>> key)
        {
            if (key.Body is UnaryExpression && ((UnaryExpression)key.Body).Operand is MemberExpression)
            {
                return ((MemberExpression)((UnaryExpression)key.Body).Operand).Member.Name;
            }
            else
            {
                throw new ArgumentException("Invalid key property.");
            }
        }

        private static bool IsIdentity<T>(string keyName)
        {
            var prop = typeof(T).GetProperty(keyName);
            if (prop != null)
            {
                var propType = prop.PropertyType;
                return prop.GetCustomAttributes(typeof(ColumnAttribute), false).Any(c => ((ColumnAttribute)c).IsDbGenerated) ||
                    typeof(int).IsAssignableFrom(propType) || typeof(long).IsAssignableFrom(propType);
            }

            return false;
        }

        private class CommandExecutor : IDataCommand
        {
            Action<string, object[]> _executor;

            public CommandExecutor(Action<string, object[]> executor)
            {
                _executor = executor;
            }

            public void Execute(string command, params object[] parameters)
            {
                _executor(command, parameters);
            }
        }

        private class CommandFactory : ISqlCommandFactory
        {
            Func<SqlCommand> _factory;

            public CommandFactory(Func<SqlCommand> factory)
            {
                _factory = factory;
            }

            public SqlCommand CreateSqlCommand()
            {
                return _factory();
            }
        }
    }

    public interface ISqlCommandFactory
    {
        SqlCommand CreateSqlCommand();
    }
}
