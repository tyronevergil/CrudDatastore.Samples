using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace CrudDatastore.MultiDbClientORM
{
    public class SqlClientCrudAdapter<T> : DelegateCrudAdapter<T> where T : EntityBase, new()
    {
        private static WhereBuilder _whereBuilder = new WhereBuilder();

        private static IEnumerable<string> _fieldList;
        private static IEnumerable<string> _fieldListWithoutKey;

        private static string _insertCommand;
        private static string _updateCommand;
        private static string _deleteCommand;
        private static string _selectCommand;

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
                    var wherePart = _whereBuilder.ToSql(predicate);
                    var sql = string.Format("{0} WHERE {1}", _selectCommand, wherePart.Sql);
                    return ExecuteQuery(sql, wherePart.Parameters);
                },

                /* read */
                (sql, parameters) =>
                {
                    var paramInDictionary = parameters.Select((Value, i) => new { Key = i.ToString(), Value }).ToDictionary(p => p.Key, p => p.Value);
                    return ExecuteQuery(sql, paramInDictionary);
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

                _selectCommand = string.Format("SELECT {1} FROM [{0}]", tableName,
                    string.Join(", ", _fieldList.Select(f => string.Format("[{0}]", f))));

                _connectionString = connectionString;
            }
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
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(string.Format("@{0}", param.Key), param.Value ?? DBNull.Value);
                    }

                    connection.Open();
                    var ret = isScalar ? command.ExecuteScalar() : command.ExecuteNonQuery();

                    return ret;
                }
            }
        }

        private static IQueryable<T> ExecuteQuery(string sql, IDictionary<string, object> parameters)
        {
            var data = new List<T>();

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(string.Format("@{0}", param.Key), param.Value ?? DBNull.Value);
                    }

                    connection.Open();
                    using (SqlDataReader dr = command.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                var t = typeof(T);
                                var entry = new T();
                                foreach (var field in _fieldList)
                                {
                                    var value = dr.GetValue(dr.GetOrdinal(field));
                                    var prop = t.GetProperty(field);
                                    prop.SetValue(entry, Convert.ChangeType(value is DBNull ? null : value, prop.PropertyType));
                                }

                                data.Add(entry);
                            }
                        }
                    }
                }
            }

            return data.AsQueryable();
        }

        /* https://gist.github.com/ryanohs/57b8c85af4f766d9c308bb58af5d68b1 */
        public class WhereBuilder
        {
            public WherePart ToSql(Expression<Func<T, bool>> expression)
            {
                var i = 0;
                return Recurse(ref i, expression.Body, isUnary: true);
            }

            public string ToRawSql(Expression<Func<T, bool>> expression)
            {
                var i = 0;
                var whereParts = Recurse(ref i, expression.Body, isUnary: true);

                if (!whereParts.Parameters.Any())
                    return whereParts.Sql;

                StringBuilder finalQuery = new StringBuilder();
                finalQuery.Append(whereParts.Sql);
                foreach (var p in whereParts.Parameters)
                {
                    var val = "@" + p.Key;
                    finalQuery = finalQuery.Replace(val, ValueToString(p.Value, false));
                }
                return finalQuery.ToString();
            }

            private WherePart Recurse(ref int i, Expression expression, bool isUnary = false, string prefix = null, string postfix = null)
            {
                if (expression is UnaryExpression)
                {
                    var unary = (UnaryExpression)expression;
                    return WherePart.Concat(NodeTypeToString(unary.NodeType), Recurse(ref i, unary.Operand, true));
                }
                if (expression is BinaryExpression)
                {
                    var body = (BinaryExpression)expression;
                    return WherePart.Concat(Recurse(ref i, body.Left), NodeTypeToString(body.NodeType), Recurse(ref i, body.Right));
                }
                if (expression is ConstantExpression)
                {
                    var constant = (ConstantExpression)expression;
                    var value = constant.Value;
                    if (value is bool && isUnary)
                    {
                        return WherePart.Concat(WherePart.IsParameter(i++, value), "=", WherePart.IsSql("1"));
                    }
                    if (value is string)
                    {
                        if (prefix == null && postfix == null)
                            value = (string)value;
                        else
                            value = prefix + (string)value + postfix;
                    }
                    return WherePart.IsParameter(i++, value);
                }
                if (expression is MemberExpression)
                {
                    var member = (MemberExpression)expression;
                    if (member.Member is PropertyInfo)
                    {
                        var property = (PropertyInfo)member.Member;
                        var colName = property.Name;
                        if (member.Expression is MemberExpression)
                        {
                            member = (MemberExpression)member.Expression;
                            if (member.Member is FieldInfo)
                            {
                                var value = GetValue(member);
                                return WherePart.IsParameter(i++, property.GetValue(value));
                            }
                        }
                        if (member.Expression is ConstantExpression)
                        {
                            var constant = (ConstantExpression)member.Expression;
                            var value = constant.Value;
                            return WherePart.IsParameter(i++, property.GetValue(value));
                        }
                        if (isUnary && member.Type == typeof(bool))
                        {
                            return WherePart.Concat(Recurse(ref i, expression), "=", WherePart.IsParameter(i++, true));
                        }
                        return WherePart.IsSql("[" + colName + "]");
                    }
                    if (member.Member is FieldInfo)
                    {
                        var value = GetValue(member);
                        if (value is string)
                        {
                            value = prefix + (string)value + postfix;
                        }
                        return WherePart.IsParameter(i++, value);
                    }
                    throw new Exception($"Expression does not refer to a property or field: {expression}");
                }
                if (expression is MethodCallExpression)
                {
                    var methodCall = (MethodCallExpression)expression;
                    // LIKE queries:
                    if (methodCall.Method == typeof(string).GetMethod("Contains", new[] { typeof(string) }))
                    {
                        return WherePart.Concat(Recurse(ref i, methodCall.Object), "LIKE", Recurse(ref i, methodCall.Arguments[0], prefix: "%", postfix: "%"));
                    }
                    if (methodCall.Method == typeof(string).GetMethod("StartsWith", new[] { typeof(string) }))
                    {
                        return WherePart.Concat(Recurse(ref i, methodCall.Object), "LIKE", Recurse(ref i, methodCall.Arguments[0], prefix: "", postfix: "%"));
                    }
                    if (methodCall.Method == typeof(string).GetMethod("EndsWith", new[] { typeof(string) }))
                    {
                        return WherePart.Concat(Recurse(ref i, methodCall.Object), "LIKE", Recurse(ref i, methodCall.Arguments[0], prefix: "%", postfix: ""));
                    }
                    // IN queries:
                    if (methodCall.Method.Name == "Contains")
                    {
                        Expression collection;
                        Expression property;
                        if (methodCall.Method.IsDefined(typeof(ExtensionAttribute)) && methodCall.Arguments.Count == 2)
                        {
                            collection = methodCall.Arguments[0];
                            property = methodCall.Arguments[1];
                        }
                        else if (!methodCall.Method.IsDefined(typeof(ExtensionAttribute)) && methodCall.Arguments.Count == 1)
                        {
                            collection = methodCall.Object;
                            property = methodCall.Arguments[0];
                        }
                        else
                        {
                            throw new Exception("Unsupported method call: " + methodCall.Method.Name);
                        }
                        var values = (IEnumerable)GetValue(collection);
                        return WherePart.Concat(Recurse(ref i, property), "IN", WherePart.IsCollection(ref i, values));
                    }
                    throw new Exception("Unsupported method call: " + methodCall.Method.Name);
                }
                throw new Exception("Unsupported expression: " + expression.GetType().Name);
            }

            private static string ValueToString(object value, bool isUnary, bool quote = true)
            {
                if (Equals(value, null))
                {
                    return "NULL";
                }

                if (value is bool)
                {
                    if (isUnary)
                    {
                        return (bool)value ? "(1=1)" : "(1=0)";
                    }
                    return (bool)value ? "1" : "0";
                }

                if (value is DateTime || value is DateTime?)
                {
                    value = (value is DateTime ? (DateTime)value : ((DateTime?)value).Value).ToString("yyyy-MM-dd HH:mm:ss.fff");
                }

                if (value is TimeSpan || value is TimeSpan?)
                {
                    value = (value is TimeSpan ? (TimeSpan)value : ((TimeSpan?)value).Value).ToString("HH:mm:ss.fff");
                }

                return IsNumeric(value) || !quote ? value.ToString() : "'" + value.ToString() + "'";
            }

            private static bool IsNumeric(object obj)
            {
                if (Equals(obj, null))
                {
                    return false;
                }

                Type objType = obj.GetType();
                objType = Nullable.GetUnderlyingType(objType) ?? objType;

                if (objType.IsPrimitive)
                {
                    return objType != typeof(bool) &&
                        objType != typeof(char) &&
                        objType != typeof(IntPtr) &&
                        objType != typeof(UIntPtr);
                }

                return objType == typeof(decimal);
            }

            private static object GetValue(Expression member)
            {
                // source: http://stackoverflow.com/a/2616980/291955
                var objectMember = Expression.Convert(member, typeof(object));
                var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                var getter = getterLambda.Compile();
                return getter();
            }

            private static string NodeTypeToString(ExpressionType nodeType)
            {
                switch (nodeType)
                {
                    case ExpressionType.Add:
                        return "+";
                    case ExpressionType.And:
                        return "&";
                    case ExpressionType.AndAlso:
                        return "AND";
                    case ExpressionType.Divide:
                        return "/";
                    case ExpressionType.Equal:
                        return "=";
                    case ExpressionType.ExclusiveOr:
                        return "^";
                    case ExpressionType.GreaterThan:
                        return ">";
                    case ExpressionType.GreaterThanOrEqual:
                        return ">=";
                    case ExpressionType.LessThan:
                        return "<";
                    case ExpressionType.LessThanOrEqual:
                        return "<=";
                    case ExpressionType.Modulo:
                        return "%";
                    case ExpressionType.Multiply:
                        return "*";
                    case ExpressionType.Negate:
                        return "-";
                    case ExpressionType.Not:
                        return "NOT";
                    case ExpressionType.NotEqual:
                        return "<>";
                    case ExpressionType.Or:
                        return "|";
                    case ExpressionType.OrElse:
                        return "OR";
                    case ExpressionType.Subtract:
                        return "-";
                }
                throw new Exception($"Unsupported node type: {nodeType}");
            }
        }

        public class WherePart
        {
            public string Sql { get; set; }
            public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

            public static WherePart IsSql(string sql)
            {
                return new WherePart()
                {
                    Parameters = new Dictionary<string, object>(),
                    Sql = sql
                };
            }

            public static WherePart IsParameter(int count, object value)
            {
                return new WherePart()
                {
                    Parameters = { { count.ToString(), value } },
                    Sql = $"@{count}"
                };
            }

            public static WherePart IsCollection(ref int countStart, IEnumerable values)
            {
                var parameters = new Dictionary<string, object>();
                var sql = new StringBuilder("(");
                foreach (var value in values)
                {
                    parameters.Add((countStart).ToString(), value);
                    sql.Append($"@{countStart},");
                    countStart++;
                }
                if (sql.Length == 1)
                {
                    sql.Append("null,");
                }
                sql[sql.Length - 1] = ')';
                return new WherePart()
                {
                    Parameters = parameters,
                    Sql = sql.ToString()
                };
            }

            public static WherePart Concat(string @operator, WherePart operand)
            {
                return new WherePart()
                {
                    Parameters = operand.Parameters,
                    Sql = $"({@operator} {operand.Sql})"
                };
            }

            public static WherePart Concat(WherePart left, string @operator, WherePart right)
            {
                return new WherePart()
                {
                    Parameters = left.Parameters.Union(right.Parameters).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    Sql = $"({left.Sql} {@operator} {right.Sql})"
                };
            }
        }
    }
}
