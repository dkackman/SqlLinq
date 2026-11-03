using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SqlLinq.TimeSeries
{
    static class Values
    {
        public static readonly IComparer<object> Comparer = new ValueComparer();

        public static double ToDouble(object value)
        {
            return Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }

        public static object SampleStandardDeviation(IList<double> values)
        {
            if (values.Count < 2)
            {
                return null;
            }

            var mean = values.Average();
            return Math.Sqrt(values.Sum(v => (v - mean) * (v - mean)) / (values.Count - 1));
        }

        public static object Percentile(IEnumerable<object> values, double p)
        {
            var sorted = values.Select(ToDouble).OrderBy(v => v).ToList();
            if (sorted.Count == 0)
            {
                return null;
            }

            var rank = p / 100 * (sorted.Count - 1);
            var lower = (int)Math.Floor(rank);
            var upper = (int)Math.Ceiling(rank);

            return sorted[lower] + (sorted[upper] - sorted[lower]) * (rank - lower);
        }

        public static double ParsePercentile(string text)
        {
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var p) || p < 0 || p > 100)
            {
                throw new SqlException(string.Format(CultureInfo.CurrentCulture, "{0} isn't a percentile between 0 and 100.", text));
            }

            return p;
        }

        public static object SortKey(object value)
        {
            return IsNumeric(value) ? ToDouble(value) : value;
        }

        public static bool IsNumeric(object value)
        {
            var code = Convert.GetTypeCode(value);

            return code >= TypeCode.SByte && code <= TypeCode.Decimal;
        }
    }
}
