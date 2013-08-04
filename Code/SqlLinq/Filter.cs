using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SqlLinq
{
    /// <summary>
    /// A simple mechanism to queue functions that modify enumerations
    /// for example where and then order by clauses get evaluated in this way
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class Filter<T>
    {
        private Queue<Func<IEnumerable<T>, IEnumerable<T>>> filters = new Queue<Func<IEnumerable<T>, IEnumerable<T>>>();

        public Filter()
        {
        }

        internal void Add(Func<IEnumerable<T>, IEnumerable<T>> filter)
        {
            if (filter != null)
                filters.Enqueue(filter);
        }

        public IEnumerable<T> Evaluate(IEnumerable<T> source)
        {
            Debug.Assert(source != null);

            foreach (var func in filters)
                source = func(source);
            
            return source;
        }
    }
}
