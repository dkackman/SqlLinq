using System.Collections.Generic;

namespace SqlLinq.TimeSeries
{
    sealed class ValueComparer : IComparer<object>
    {
        public int Compare(object x, object y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            if (Values.IsNumeric(x) && Values.IsNumeric(y))
            {
                return Values.ToDouble(x).CompareTo(Values.ToDouble(y));
            }

            return Comparer<object>.Default.Compare(x, y);
        }
    }
}
