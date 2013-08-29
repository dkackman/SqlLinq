using System;
using System.Diagnostics;

namespace SqlLinq.SyntaxTree.Joins
{
    /// <summary>
    /// This guy acts as part of the result selector in the composed expression that
    /// calls Enumerable.Join. It knows how to look up both the inner and outer result valuues
    /// as the join result is being created for each joined result row
    /// </summary>
    class JoinResultSelector<TOuter, TInner>
    {
        private IJoinEntry _outer;
        private IJoinEntry _inner;

        public JoinResultSelector(object outer, object inner)
        {
            Debug.Assert(outer != null);
            Debug.Assert(inner != null);

            _outer = CreateEntry<TOuter>(outer);
            _inner = CreateEntry<TInner>(inner);
        }

        private static IJoinEntry CreateEntry<T>(object o)
        {
            if (o.GetType().IsDictionary())
                return new DictionaryJoinEntry(o, typeof(T));

            return new ObjectJoinEntry(o);
        }

        public object this[string key]
        {
            get
            {
                // if the key is prefixed we know whether to go for the inner or outer specifically
                if (key.StartsWith("this.", StringComparison.OrdinalIgnoreCase))
                    return _outer[key.Substring(key.IndexOf(".") + 1)];

                if (key.StartsWith("that.", StringComparison.OrdinalIgnoreCase))
                    return _inner[key.Substring(key.IndexOf(".") + 1)];

                // otherwise look at the outer object first and then the inner
                if (_outer.ContainsKey(key))
                    return _outer[key];

                if (_inner.ContainsKey(key))
                    return _inner[key];

                throw new ArgumentException("An item with the specified key does not exist " + key);
            }
        }
    }
}
