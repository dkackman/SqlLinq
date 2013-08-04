using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlLinq.UnitTests
{
    public class DictionaryComparer<TKey, TValue> : IEqualityComparer<IDictionary<TKey, TValue>>
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
                foreach (KeyValuePair<TKey, TValue> pair in obj)
                {
                    int key = pair.Key.GetHashCode();
                    int value = pair.Value != null ? pair.Value.GetHashCode() : 0;
                    hash ^= key ^ value;
                }
            }
            return hash;
        }
    }
}
