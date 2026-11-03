using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlLinq.TimeSeries
{
    sealed class AggregateFactory<T>
    {
        public static readonly HashSet<string> Names = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "count", "sum", "average", "avg", "min", "max", "stddev", "uniqueCount", "percentile", "median", "latest", "earliest", "rate"
        };

        private readonly Func<string, Func<T, object>> _compile;

        public AggregateFactory(Func<string, Func<T, object>> compile)
        {
            _compile = compile;
        }

        public Aggregate<T> Create(string name, string argText)
        {
            var args = SqlSyntax.SplitTopLevel(argText);

            switch (name.ToLowerInvariant())
            {
                case "count":
                    if (args.Count == 1 && args[0] == "*")
                    {
                        return (rows, window) => rows.Count;
                    }
                    return Over(name, args, values => values.Count);
                case "uniquecount":
                    return Over(name, args, values => values.Distinct().Count());
                case "sum":
                    return Over(name, args, values => values.Sum(Values.ToDouble));
                case "average":
                case "avg":
                    return Over(name, args, values => values.Count == 0 ? null : (object)values.Average(Values.ToDouble));
                case "min":
                    return Over(name, args, values => values.Count == 0 ? null : values.Aggregate((a, b) => Values.Comparer.Compare(b, a) < 0 ? b : a));
                case "max":
                    return Over(name, args, values => values.Count == 0 ? null : values.Aggregate((a, b) => Values.Comparer.Compare(b, a) > 0 ? b : a));
                case "stddev":
                    return Over(name, args, values => Values.SampleStandardDeviation(values.Select(Values.ToDouble).ToList()));
                case "median":
                    return Over(name, args, values => Values.Percentile(values, 50));
                case "percentile":
                    if (args.Count != 2)
                    {
                        throw new SqlException("percentile takes (value, percentile) here, e.g. percentile(RPM, 95).");
                    }
                    var percentile = Values.ParsePercentile(args[1]);

                    return Over(name, args.Take(1).ToList(), values => Values.Percentile(values, percentile));
                case "latest":
                    return ByTime(name, args, (time, best) => time >= best);
                case "earliest":
                    return ByTime(name, args, (time, best) => time < best);
                case "rate":
                    return Rate(args);
            }
            throw new SqlException(name + " isn't an aggregate function.");
        }

        private Aggregate<T> Over(string name, List<string> args, Func<List<object>, object> aggregate)
        {
            if (args.Count != 1 || args[0].Length == 0)
            {
                throw new SqlException(name + " takes one value, e.g. " + name + "(RPM).");
            }

            var value = _compile(args[0]);
            return (rows, window) => aggregate(rows.Select(r => value(r.Item)).Where(v => v != null).ToList());
        }

        private Aggregate<T> ByTime(string name, List<string> args, Func<TimeSpan, TimeSpan, bool> better)
        {
            if (args.Count != 1)
            {
                throw new SqlException(name + " takes one value, e.g. " + name + "(ECT).");
            }

            var value = _compile(args[0]);
            return (rows, window) =>
            {
                object result = null;
                var at = TimeSpan.Zero;

                foreach (var (Time, Item) in rows)
                {
                    var v = value(Item);
                    if (v != null && (result == null || better(Time, at)))
                    {
                        result = v;
                        at = Time;
                    }
                }

                return result;
            };
        }

        private Aggregate<T> Rate(List<string> args)
        {
            var calls = args.Count == 2 ? SqlSyntax.FindCalls(args[0], Names) : null;
            if (calls == null || !SqlSyntax.IsWholeCall(args[0], calls))
            {
                throw new SqlException("rate takes (aggregate, duration), e.g. rate(count(*), 1 second).");
            }

            var inner = Create(calls[0].Name, calls[0].Args);

            var per = Bound.Parse(args[1], "rate").Offset.Ticks;
            if (per <= 0)
            {
                throw new SqlException("rate needs a positive duration, e.g. rate(count(*), 1 second).");
            }

            return (rows, window) =>
            {
                var value = inner(rows, window);

                return value == null || window <= TimeSpan.Zero ? null : (object)(Values.ToDouble(value) * per / window.Ticks);
            };
        }
    }
}
