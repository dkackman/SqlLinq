using System.Collections.Generic;

namespace SqlLinq
{
    class DictionaryComparer<TKey, TValue> : IEqualityComparer<IDictionary<TKey, TValue>>
    {
        public DictionaryComparer()
        {
        }

        public bool Equals(IDictionary<TKey, TValue> x, IDictionary<TKey, TValue> y)
        {
            if (x.Count != y.Count)
                return false;

            return GetHashCode(x) == GetHashCode(y);
        }

        public int GetHashCode(IDictionary<TKey, TValue> obj)
        {
            int hash = 0;
            unchecked
            {
                Dictionary<TKey, TValue> d = obj as Dictionary<TKey, TValue>;
                IEqualityComparer<TKey> comparer = d != null ? d.Comparer : EqualityComparer<TKey>.Default;

                foreach (KeyValuePair<TKey, TValue> pair in obj)
                {
                    int key = comparer.GetHashCode(pair.Key);
                    int value = pair.Value != null ? pair.Value.GetHashCode() : 0;
                    hash ^= key ^ value;
                }
            }
            return hash;
        }
    }
}
