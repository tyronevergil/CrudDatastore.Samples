using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using CrudDatastore;

namespace CrudDatastore.SqlClient
{
    public class SqlClientCrudAdapter<T> : DelegateCrudAdapter<T> where T : EntityBase
    {
        private readonly DelegateQueryAdapter<T> _queryAdapter;

        private static IEnumerable<string> _fieldList;
        private static IEnumerable<string> _fieldListWithoutKey;

        private static string _insertCommand;
        private static string _updateCommand;
        private static string _deleteCommand;

        private static string _connectionString;

        public SqlClientCrudAdapter(string connectionString)
            : this(connectionString, GetTableName())
        { }

        public SqlClientCrudAdapter(string connectionString, string tableName)
            : this(connectionString, tableName, GetPropertyKey())
        { }

        public SqlClientCrudAdapter(string connectionString, string tableName, Expression<Func<T, object>> key)
            : this(connectionString, tableName, key, IsIdentity(GetPropertyKeyName(key)))
        { }

        public SqlClientCrudAdapter(string connectionString, string tableName, Expression<Func<T, object>> key, bool isIdentity)
            : this(connectionString, tableName, GetPropertyKeyName(key), isIdentity)
        { }

        private SqlClientCrudAdapter(string connectionString, string tableName, string keyName, bool isIdentity)
            : base
            (
                /* create */
                (e) =>
                {
                    var t = typeof(T);
                    var parameters = (isIdentity ? _fieldListWithoutKey : _fieldList)
                        .Select(f => new { Key = f, Value = t.GetProperty(f).GetValue(e) }).ToDictionary(p => p.Key, p => p.Value);
                    var ret = Execute(_insertCommand, parameters, isIdentity);
                    if (isIdentity)
                    {
                        var prop = t.GetProperty(keyName);
                        prop.SetValue(e, Convert.ChangeType(ret, prop.PropertyType));
                    }
                },

                /* update */
                (e) =>
                {
                    var t = typeof(T);
                    var parameters = _fieldList.Select(f => new { Key = f, Value = t.GetProperty(f).GetValue(e) }).ToDictionary(p => p.Key, p => p.Value);
                    Execute(_updateCommand, parameters);
                },

                /* delete */
                (e) =>
                {
                    var parameters = new Dictionary<string, object>() { { keyName, typeof(T).GetProperty(keyName).GetValue(e) } };
                    Execute(_deleteCommand, parameters);
                },

                /* read */
                (predicate) =>
                {
                    return Enumerable.Empty<T>().AsQueryable();
                },

                /* read */
                (sql, parameters) =>
                {
                    return Enumerable.Empty<T>().AsQueryable();
                }
            )
        {
            if (_fieldList == null)
            {
                _fieldList = typeof(T).GetProperties().Where(p => p.PropertyType.IsSealed && p.GetAccessors().Any(a => !(a.IsVirtual && !a.IsFinal) && a.ReturnType == typeof(void))).Select(p => p.Name).ToList();
                _fieldListWithoutKey = _fieldList.Where(f => !string.Equals(f, keyName, StringComparison.OrdinalIgnoreCase)).ToList();

                _insertCommand = string.Format("INSERT INTO [{0}] ({1}) VALUES ({2}){3}", tableName,
                    string.Join(", ", _fieldListWithoutKey.Select(f => string.Format("[{0}]", f))),
                    string.Join(", ", _fieldListWithoutKey.Select(f => string.Format("@{0}", f))),
                    isIdentity ? "; SELECT CAST(SCOPE_IDENTITY() AS INT);" : "");

                _updateCommand = string.Format("UPDATE [{0}] SET {1} WHERE {2}", tableName,
                    string.Join(", ", _fieldListWithoutKey.Select(f => string.Format("[{0}] = @{0}", f))),
                    string.Format("[{0}] = @{0}", keyName));

                _deleteCommand = string.Format("DELETE [{0}] WHERE {1}", tableName,
                    string.Format("[{0}] = @{0}", keyName));

                _connectionString = connectionString;
            }

            var sqlClientQueryAdapterType = typeof(SqlClientQueryAdapter<>).MakeGenericType(new[] { typeof(T) });
            _queryAdapter = (DelegateQueryAdapter<T>)Activator.CreateInstance(sqlClientQueryAdapterType, connectionString, tableName);
        }

        private static string GetTableName()
        {
            return typeof(T).Name;
        }

        private static Expression<Func<T, object>> GetPropertyKey()
        {
            var possibleKeys = new[] { "Id", string.Format("{0}Id", GetTableName()) };
            var keys = typeof(T).GetProperties()
                .Where(p => possibleKeys.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
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

        private static string GetPropertyKeyName(Expression<Func<T, object>> key)
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

        private static bool IsIdentity(string keyName)
        {
            var prop = typeof(T).GetProperty(keyName);
            if (prop != null)
            {
                return typeof(int).IsAssignableFrom(prop.PropertyType) || typeof(long).IsAssignableFrom(prop.PropertyType);
            }

            return false;
        }

        private static object Execute(string sql, IDictionary<string, object> parameters, bool isScalar = false)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(sql, connection))
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(string.Format("@{0}", param.Key), param.Value ?? DBNull.Value);
                    }

                    connection.Open();
                    return isScalar ? command.ExecuteScalar() : command.ExecuteNonQuery();
                }
            }
        }

        public override IQueryable<T> Execute(Expression<Func<T, bool>> predicate)
        {
            return _queryAdapter.Execute(predicate);
        }

        public override IQueryable<T> Execute(string sql, params object[] parameters)
        {
            return _queryAdapter.Execute(sql, parameters);
        }
    }
}
