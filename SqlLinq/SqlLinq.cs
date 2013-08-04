using System.Collections.Generic;
using System.Diagnostics;

namespace SqlLinq
{
    public static class SqlLinq
    {
        public static TSource QueryScalar<TSource>(this IEnumerable<TSource> enumerable, string sql)
        {
            Debug.Assert(enumerable != null);
            Debug.Assert(string.IsNullOrEmpty(sql) == false, "Sql cannot be empty");

            return enumerable.QueryScalar<TSource, TSource>(sql);
        }

        public static TResult QueryScalar<TSource, TResult>(this IEnumerable<TSource> enumerable, string sql)
        {
            Debug.Assert(enumerable != null);
            Debug.Assert(string.IsNullOrEmpty(sql) == false, "Sql cannot be empty");

            ScalarQuery<TSource, TResult> query = new ScalarQuery<TSource, TResult>(sql);
            query.Compile();
            return enumerable.QueryScalar<TSource, TResult>(query);
        }

        public static TResult QueryScalar<TSource, TResult>(this IEnumerable<TSource> enumerable, ScalarQuery<TSource, TResult> query)
        {
            Debug.Assert(enumerable != null);
            Debug.Assert(query != null);

            return query.Evaluate(enumerable);
        }

        public static IEnumerable<TSource> Query<TSource>(this IEnumerable<TSource> enumerable, string sql)
        {
            Debug.Assert(enumerable != null);
            Debug.Assert(string.IsNullOrEmpty(sql) == false, "Sql cannot be empty");

            return enumerable.Query<TSource, TSource>(sql);
        }

        public static IEnumerable<TResult> Query<TSource, TResult>(this IEnumerable<TSource> enumerable, string sql)
        {
            Debug.Assert(enumerable != null);
            Debug.Assert(string.IsNullOrEmpty(sql) == false, "Sql cannot be empty");

            Query<TSource, TResult> query = new Query<TSource, TResult>(sql);
            query.Compile();
            return enumerable.Query<TSource, TResult>(query);
        }

        public static IEnumerable<TResult> Query<TSource, TResult>(this IEnumerable<TSource> enumerable, Query<TSource, TResult> query)
        {
            Debug.Assert(enumerable != null);
            Debug.Assert(query != null);

            return query.Evaluate(enumerable);
        }

        public static TResult QueryScalar<TSource, TInner, TResult>(this IEnumerable<TSource> enumerable, string sql, IEnumerable<TInner> inner)
        {
            Debug.Assert(enumerable != null);
            Debug.Assert(inner != null);
            Debug.Assert(string.IsNullOrEmpty(sql) == false, "Sql cannot be empty");

            ScalarJoinQuery<TSource, TInner, TResult> query = new ScalarJoinQuery<TSource, TInner, TResult>(sql);
            query.Compile();
            return query.Evaluate(enumerable, inner);
        }

        public static IEnumerable<TResult> Query<TSource, TInner, TResult>(this IEnumerable<TSource> enumerable, string sql, IEnumerable<TInner> inner)
        {
            Debug.Assert(enumerable != null);
            Debug.Assert(inner != null);
            Debug.Assert(string.IsNullOrEmpty(sql) == false, "Sql cannot be empty");

            JoinQuery<TSource, TInner, TResult> query = new JoinQuery<TSource, TInner, TResult>(sql);
            query.Compile();
            return query.Evaluate(enumerable, inner);
        }
    }
}
