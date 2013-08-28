using System.Collections.Generic;

namespace SqlLinq
{
    class ExpandoComparer : EqualityComparer<object>
    {
        private DictionaryComparer<string, object> _comparer = new DictionaryComparer<string, object>();

        public override bool Equals(object x, object y)
        {
            return _comparer.Equals(x as IDictionary<string, object>, y as IDictionary<string, object>);
        }

        public override int GetHashCode(object obj)
        {
            return _comparer.GetHashCode(obj as IDictionary<string, object>);
        }
    }
}
