using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CrudDatastore;

namespace CrudDatastore.SqlClient
{
    public class InMemoryQueryAdapter<T> : DelegateQueryAdapter<T> where T : EntityBase, new()
    {
        private static IEnumerable<string> _fieldList;

        public InMemoryQueryAdapter(IEnumerable<T> source)
            : base
            (
                /* read */
                (predicate) =>
                {
                    return ExecuteQuery(source, predicate);
                },

                (sql, parameters) =>
                {
                    return ExecuteQuery(source, e => false);
                }
            )
        {
            if (_fieldList == null)
            {
                _fieldList = typeof(T).GetProperties().Where(p => p.PropertyType.IsSealed && p.GetAccessors().Any(a => !(a.IsVirtual && !a.IsFinal) && a.ReturnType == typeof(void))).Select(p => p.Name).ToList();
            }
        }

        private static IQueryable<T> ExecuteQuery(IEnumerable<T> source, Expression<Func<T, bool>> predicate)
        {
            var query = source.Where(predicate.Compile());
            return query.Select(CreateCopy).ToList().AsQueryable();
        }

        private static T CreateCopy(T source)
        {
            var t = typeof(T);
            var entry = new T();
            foreach (var field in _fieldList)
            {
                var f = t.GetProperty(field);
                f.SetValue(entry, f.GetValue(source));
            }

            return entry;
        }
    }
}
