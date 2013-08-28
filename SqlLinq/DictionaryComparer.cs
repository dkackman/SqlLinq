using System;
using System.Linq;
using System.Collections.Generic;

namespace SqlLinq
{
    public class DictionaryComparer<TKey, TValue> : EqualityComparer<IDictionary<TKey, TValue>>
    {
        public DictionaryComparer()
        {
        }
        public override bool Equals(IDictionary<TKey, TValue> x, IDictionary<TKey, TValue> y)
        {
            // early-exit checks
            if (object.ReferenceEquals(x, y))
                return true;

            if (null == x || y == null)
                return false;

            if (x.Count != y.Count)
                return false;

            // check keys are the same
            foreach (TKey k in x.Keys)
                if (!y.ContainsKey(k))
                    return false;

            // check values are the same
            foreach (TKey k in x.Keys)
            {
                TValue v = x[k];
                if (object.ReferenceEquals(v, null))
                    return object.ReferenceEquals(y[k], null);

                if (!v.Equals(y[k]))
                    return false;
            }
            return true;
        }

        public override int GetHashCode(IDictionary<TKey, TValue> obj)
        {
            if (obj == null)
                return 0;

            int hash = 0;

            foreach (KeyValuePair<TKey, TValue> pair in obj)
            {
                int key = pair.Key.GetHashCode(); // key cannot be null
                int value = pair.Value != null ? pair.Value.GetHashCode() : 0;
                hash ^= ShiftAndWrap(key, 2) ^ value;
            }

            return hash;
        }

        private int ShiftAndWrap(int value, int positions)
        {
            positions = positions & 0x1F;

            // Save the existing bit pattern, but interpret it as an unsigned integer. 
            uint number = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
            // Preserve the bits to be discarded. 
            uint wrapped = number >> (32 - positions);
            // Shift and wrap the discarded bits. 
            return BitConverter.ToInt32(BitConverter.GetBytes((number << positions) | wrapped), 0);
        }
    }
}
