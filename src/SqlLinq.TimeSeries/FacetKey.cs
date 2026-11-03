using System.Collections;

namespace SqlLinq.TimeSeries
{
    sealed class FacetKey
    {
        public FacetKey(object[] values)
        {
            Values = values;
        }

        public object[] Values { get; }

        public override bool Equals(object obj)
        {
            return obj is FacetKey other && StructuralComparisons.StructuralEqualityComparer.Equals(Values, other.Values);
        }

        public override int GetHashCode()
        {
            return StructuralComparisons.StructuralEqualityComparer.GetHashCode(Values);
        }
    }
}
