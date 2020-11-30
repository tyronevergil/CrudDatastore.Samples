using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CrudDatastore;

namespace CrudDatastore.SqlClient
{
    public class InMemoryCrudAdapter<T> : DelegateCrudAdapter<T> where T : EntityBase, new() 
    {
        private readonly DelegateQueryAdapter<T> _queryAdapter;

        private static IEnumerable<string> _fieldList;
        private static IEnumerable<string> _fieldListWithoutKey;

        public InMemoryCrudAdapter(IList<T> source)
            : this(source, GetPropertyKey())
        { }

        public InMemoryCrudAdapter(IList<T> source, Expression<Func<T, object>> key)
            : this(source, key, IsIdentityKey(GetPropertyKeyName(key)))
        { }

        public InMemoryCrudAdapter(IList<T> source, Expression<Func<T, object>> key, bool isIdentity)
            : this(source, GetPropertyKeyName(key), isIdentity)
        { }

        private InMemoryCrudAdapter(IList<T> source, string keyName, bool isIdentity)
            : base
            (
                /* create */
                (e) =>
                {
                    var t = typeof(T);

                    if (isIdentity)
                    {
                        var param = Expression.Parameter(t, "e");
                        var prop = Expression.Property(param, keyName);

                        var selector = Expression.Lambda(prop, param);

                        var nextId = (source.Any() ? source.Max((Func<T, int>)selector.Compile()) : 0) + 1;
                        t.GetProperty(keyName).SetValue(e, nextId);
                    }

                    var entry = new T();
                    foreach (var field in _fieldList)
                    {
                        var f = t.GetProperty(field);
                        f.SetValue(entry, f.GetValue(e));
                    }

                    source.Add(entry);
                },

                /* update */
                (e) =>
                {
                    var entry = source.FirstOrDefault(CreatePredicate(e, keyName));
                    if (entry != null)
                    {
                        var t = typeof(T);
                        foreach (var field in _fieldListWithoutKey)
                        {
                            var f = t.GetProperty(field);
                            f.SetValue(entry, f.GetValue(e));
                        }
                    }
                },

                /* delete */
                (e) =>
                {
                    var entry = source.FirstOrDefault(CreatePredicate(e, keyName));
                    if (entry != null)
                    {
                        source.Remove(entry);
                    }
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
            }

            var inMemoryQueryAdapterType = typeof(InMemoryQueryAdapter<>).MakeGenericType(new[] { typeof(T) });
            _queryAdapter = (DelegateQueryAdapter<T>)Activator.CreateInstance(inMemoryQueryAdapterType, source);
        }

        private static Expression<Func<T, object>> GetPropertyKey()
        {
            var type = typeof(T);
            var possibleKeys = new[] { "Id", string.Format("{0}Id", type.Name) };
            var keys = type.GetProperties()
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

        private static bool IsIdentityKey(string keyName)
        {
            var prop = typeof(T).GetProperty(keyName);
            if (prop != null)
            {
                return typeof(int).IsAssignableFrom(prop.PropertyType) || typeof(long).IsAssignableFrom(prop.PropertyType);
            }

            return false;
        }

        private static Func<T, bool> CreatePredicate(T entry, string keyName)
        {
            var t = typeof(T);

            var param = Expression.Parameter(t, "e");
            var prop = Expression.Property(param, keyName);
            var value = Expression.Constant(t.GetProperty(keyName).GetValue(entry));

            var predicate = Expression.Lambda(Expression.Equal(prop, value), param);

            return (Func<T, bool>)predicate.Compile();
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
