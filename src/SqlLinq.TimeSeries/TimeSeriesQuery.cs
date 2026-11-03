using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace SqlLinq.TimeSeries
{
    public class TimeSeriesQuery<T>
    {
        private static readonly Regex Keywords = new Regex(@"'[^']*'|\[[^\]]*\]|\b(WHERE|FACET|SINCE|UNTIL|TIMESERIES|LIMIT|COMPARE|GROUP|HAVING|ORDER)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex SelectFrom = new Regex(@"^\s*SELECT\s+(.+?)\s+FROM\s+(\[[^\]]+\]|\w+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex Alias = new Regex(@"^(.+?)\s+AS\s+(\[[^\]]+\]|\w+)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex CompareWith = new Regex(@"^WITH\s+(.+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline);

        private readonly Func<T, TimeSpan> _time;
        private readonly Func<string, string> _rewrite;
        private readonly Query<T, T> _where;
        private readonly List<(string Text, string Name, Func<T, object> Value)> _facets = new List<(string, string, Func<T, object>)>();
        private readonly List<(string Name, Aggregate<T> Aggregate)> _aggregates = new List<(string, Aggregate<T>)>();
        private readonly List<(string Name, Func<T, object> Value)> _columns = new List<(string, Func<T, object>)>();
        private readonly Bound _since;
        private readonly Bound _until;
        private readonly TimeSpan? _compare;
        private readonly long? _bucketTicks;
        private readonly int? _limit;

        public TimeSeriesQuery(string nrql, Func<T, TimeSpan> time, Func<string, string> rewrite = null)
        {
            _time = time ?? throw new ArgumentNullException(nameof(time));
            _rewrite = rewrite ?? (expression => expression);

            var clauses = SplitClauses(nrql, out var head);
            var select = SelectFrom.Match(head);
            if (!select.Success)
            {
                throw new SqlException("A time series query starts SELECT <columns> FROM <name>.");
            }

            if (clauses.TryGetValue("FACET", out var facets))
            {
                foreach (var facet in SqlSyntax.SplitTopLevel(facets))
                {
                    _facets.Add((facet, FacetExpression.Name(facet), FacetExpression.Compile<T>(facet, _rewrite)));
                }
            }

            var items = SqlSyntax.SplitTopLevel(select.Groups[1].Value);
            var aggregated = items.Any(i => SqlSyntax.FindCalls(i, AggregateFactory<T>.Names).Count > 0);
            var factory = new AggregateFactory<T>(Compile);
            foreach (var item in items)
            {
                AddColumn(item, aggregated, factory);
            }

            if (clauses.TryGetValue("WHERE", out var where))
            {
                _where = new Query<T, T>("SELECT * FROM this WHERE " + _rewrite(where));
                _where.Compile();
            }

            if (clauses.TryGetValue("SINCE", out var since))
            {
                _since = Bound.Parse(since, "SINCE");
            }

            if (clauses.TryGetValue("UNTIL", out var until))
            {
                _until = Bound.Parse(until, "UNTIL");
            }

            if (clauses.TryGetValue("COMPARE", out var compare))
            {
                var with = CompareWith.Match(compare);
                if (!with.Success)
                {
                    throw new SqlException("COMPARE WITH needs a duration, e.g. COMPARE WITH 1 minute ago.");
                }
                if (_since == null)
                {
                    throw new SqlException("COMPARE WITH needs a SINCE window to compare.");
                }

                _compare = Bound.Parse(with.Groups[1].Value, "COMPARE WITH").Offset;
            }

            if (clauses.TryGetValue("TIMESERIES", out var bucket))
            {
                var size = Bound.Parse(bucket, "TIMESERIES");
                if (size.Ago || size.Offset <= TimeSpan.Zero)
                {
                    throw new SqlException("TIMESERIES needs a positive duration, e.g. TIMESERIES 1 second.");
                }

                _bucketTicks = size.Offset.Ticks;
            }

            if (clauses.TryGetValue("LIMIT", out var limit))
            {
                if (!int.TryParse(limit, NumberStyles.None, CultureInfo.InvariantCulture, out var n) || n < 1)
                {
                    throw new SqlException("LIMIT needs a positive whole number.");
                }
                if (aggregated && _facets.Count == 0)
                {
                    throw new SqlException("LIMIT needs FACET (top facets) or a SELECT without aggregates (first rows).");
                }

                _limit = n;
            }

            if (!aggregated && (_facets.Count > 0 || _bucketTicks != null))
            {
                throw new SqlException("FACET and TIMESERIES need aggregate functions in SELECT, e.g. average(RPM).");
            }
        }

        public IList<IDictionary<string, object>> Evaluate(IEnumerable<T> source)
        {
            var all = source.Select(item => (Time: _time(item), Item: item)).ToList();
            var result = new List<IDictionary<string, object>>();
            if (all.Count == 0)
            {
                return result;
            }

            var newest = all.Max(r => r.Time);
            var from = _since?.Resolve(newest) ?? all.Min(r => r.Time);
            var to = _until?.Resolve(newest) ?? newest + TimeSpan.FromTicks(1);

            Run(all, from, to, _compare == null ? null : "current", result);

            if (_compare != null)
            {
                Run(all, from - _compare.Value, to - _compare.Value, "previous", result);
            }

            return result;
        }

        private Func<T, object> Compile(string expression)
        {
            return SqlExpression.Compile<T>(_rewrite(expression));
        }

        private static Dictionary<string, string> SplitClauses(string nrql, out string head)
        {
            var clauses = new Dictionary<string, string>();
            var keywords = Keywords.Matches(nrql).Where(m => m.Groups[1].Success).ToList();

            for (var i = 0; i < keywords.Count; i++)
            {
                var keyword = keywords[i].Groups[1].Value.ToUpperInvariant();
                if (keyword == "GROUP" || keyword == "HAVING" || keyword == "ORDER")
                {
                    throw new SqlException(keyword + " isn't supported in a time series query. Use FACET to group.");
                }

                var start = keywords[i].Index + keywords[i].Length;
                var end = i + 1 < keywords.Count ? keywords[i + 1].Index : nrql.Length;
                if (!clauses.TryAdd(keyword, nrql.Substring(start, end - start).Trim()))
                {
                    throw new SqlException("The query has more than one " + keyword + " clause.");
                }
            }

            head = keywords.Count > 0 ? nrql.Substring(0, keywords[0].Index) : nrql;
            return clauses;
        }

        private void AddColumn(string item, bool aggregated, AggregateFactory<T> factory)
        {
            var alias = Alias.Match(item);
            var text = alias.Success ? alias.Groups[1].Value.Trim() : item;
            var name = alias.Success ? SqlSyntax.Unbracket(alias.Groups[2].Value) : item;

            if (text.Length == 0)
            {
                throw new SqlException("SELECT has an empty column.");
            }

            if (!aggregated)
            {
                _columns.Add((name, Compile(text)));
                return;
            }

            var calls = SqlSyntax.FindCalls(text, AggregateFactory<T>.Names);
            if (calls.Count == 0)
            {
                if (_facets.Any(f => f.Text.Equals(text, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }

                throw new SqlException(string.Format(CultureInfo.CurrentCulture, "'{0}' isn't an aggregate. Alongside aggregates, plain columns must be FACET fields.", text));
            }

            if (SqlSyntax.IsWholeCall(text, calls))
            {
                var args = SqlSyntax.SplitTopLevel(calls[0].Args);
                if (calls[0].Name.Equals("percentile", StringComparison.OrdinalIgnoreCase) && args.Count > 2)
                {
                    foreach (var p in args.Skip(1))
                    {
                        _aggregates.Add((alias.Success ? name + "." + p : string.Format(CultureInfo.CurrentCulture, "percentile({0}, {1})", args[0], p), factory.Create("percentile", args[0] + ", " + p)));
                    }
                }
                else
                {
                    _aggregates.Add((name, factory.Create(calls[0].Name, calls[0].Args)));
                }

                return;
            }

            var inner = calls.Select(c => factory.Create(c.Name, c.Args)).ToList();
            var outer = SqlExpression.Compile<IDictionary<string, double>>(_rewrite(SqlSyntax.ReplaceCalls(text, calls, i => "agg" + i)));

            object combined(IReadOnlyList<(TimeSpan Time, T Item)> rows, TimeSpan window)
            {
                var values = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < inner.Count; i++)
                {
                    var value = inner[i](rows, window);
                    if (value == null)
                    {
                        return null;
                    }

                    values["agg" + i] = Values.ToDouble(value);
                }

                return outer(values);
            }
            _aggregates.Add((name, combined));
        }

        private void Run(List<(TimeSpan Time, T Item)> all, TimeSpan from, TimeSpan to, string comparison, List<IDictionary<string, object>> result)
        {
            var rows = all.Where(r => r.Time >= from && r.Time < to).ToList();
            if (_where != null)
            {
                rows = _where.Evaluate(rows.Select(r => r.Item)).Select(item => (Time: _time(item), Item: item)).ToList();
            }

            if (_aggregates.Count == 0)
            {
                foreach (var r in rows.OrderBy(r => r.Time).Take(_limit ?? int.MaxValue))
                {
                    var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    if (comparison != null)
                    {
                        row["Comparison"] = comparison;
                    }

                    row["Time"] = r.Time;
                    foreach (var column in _columns)
                    {
                        row[column.Name] = column.Value(r.Item);
                    }

                    result.Add(row);
                }

                return;
            }

            var window = to - from;
            var groups = _facets.Count == 0
                ? new List<(object[] Key, List<(TimeSpan Time, T Item)> Rows)> { (Array.Empty<object>(), rows) }
                : (
                    from r in rows
                    group r by new FacetKey(_facets.Select(f => f.Value(r.Item)).ToArray()) into g
                    let groupRows = g.ToList()
                    orderby Values.SortKey(_aggregates[0].Aggregate(groupRows, window)) descending
                    select (Key: g.Key.Values, Rows: groupRows)).Take(_limit ?? int.MaxValue).ToList();

            if (_bucketTicks == null)
            {
                foreach (var (Key, Rows) in groups)
                {
                    result.Add(CreateRow(Key, Rows, window, comparison, null));
                }

                return;
            }

            var ticks = _bucketTicks.Value;
            var first = Index(from, ticks);
            var last = Index(to - TimeSpan.FromTicks(1), ticks);
            var buckets = groups.Select(g => g.Rows.ToLookup(r => Index(r.Time, ticks))).ToList();

            for (var b = first; b <= last; b++)
            {
                for (var i = 0; i < groups.Count; i++)
                {
                    result.Add(CreateRow(groups[i].Key, buckets[i][b].ToList(), TimeSpan.FromTicks(ticks), comparison, b * ticks));
                }
            }
        }

        private Dictionary<string, object> CreateRow(object[] key, IReadOnlyList<(TimeSpan Time, T Item)> rows, TimeSpan window, string comparison, long? beginTicks)
        {
            var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            if (beginTicks != null)
            {
                row["BeginTime"] = new TimeSpan(beginTicks.Value);
                row["EndTime"] = new TimeSpan(beginTicks.Value) + window;
            }

            if (comparison != null)
            {
                row["Comparison"] = comparison;
            }

            for (var i = 0; i < _facets.Count; i++)
            {
                row[_facets[i].Name] = key[i];
            }

            foreach (var aggregate in _aggregates)
            {
                row[aggregate.Name] = aggregate.Aggregate(rows, window);
            }

            return row;
        }

        private static long Index(TimeSpan time, long ticks)
        {
            return (long)Math.Floor((double)time.Ticks / ticks);
        }
    }
}
