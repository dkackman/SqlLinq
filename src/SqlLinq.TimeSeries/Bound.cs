using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SqlLinq.TimeSeries
{
    sealed class Bound
    {
        private static readonly Regex Duration = new Regex(@"^(\d+(?:\.\d+)?)\s*(?:(ms|milliseconds?)|(m|mins?|minutes?)|(h|hours?)|s|secs?|seconds?)?\s*(ago)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public TimeSpan Offset { get; private set; }

        public bool Ago { get; private set; }

        public TimeSpan Resolve(TimeSpan now)
        {
            return Ago ? now - Offset : Offset;
        }

        public static Bound Parse(string text, string clause)
        {
            var m = Duration.Match(text);
            if (!m.Success)
            {
                throw new SqlException(string.Format(CultureInfo.CurrentCulture, "{0} {1} isn't a duration, e.g. {0} 30 seconds or {0} 500 ms.", clause, text));
            }

            var value = double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);

            TimeSpan offset;

            if (m.Groups[2].Success)
            {
                offset = TimeSpan.FromMilliseconds(value);
            }
            else if (m.Groups[3].Success)
            {
                offset = TimeSpan.FromMinutes(value);
            }
            else if (m.Groups[4].Success)
            {
                offset = TimeSpan.FromHours(value);
            }
            else
            {
                offset = TimeSpan.FromSeconds(value);
            }

            return new Bound { Offset = offset, Ago = m.Groups[5].Success };
        }
    }
}
