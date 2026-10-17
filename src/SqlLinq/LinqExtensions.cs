using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlLinq
{
    static class LinqExtensions
    {
        /// <summary>
        /// Allows for a Where query where the predicate is a function of the input enumerable 
        /// (i.e. is a subquery such as WHERE x > (SELECT AVG(x))
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">An System.Collections.Generic.IEnumerable<T> to filter.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns> An System.Collections.Generic.IEnumerable<T> that contains elements from
        ///     the input sequence that satisfy the condition.</returns>
        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<IEnumerable<TSource>, TSource, bool> predicate)
        {
            return source.Where(t => predicate(source, t));
        }

        public static int IndexOf<T>(this IEnumerable<T> enumerable, T item)
        {
            int i = 0;
            foreach (T t in enumerable)
            {
                if (object.ReferenceEquals(t, item))
                    return i;
                i++;
            }

            return -1;
        }
    }
}
