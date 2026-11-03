using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace SqlLinq.TimeSeries
{
    public static class FacetExpression
    {
        private static readonly HashSet<string> BucketNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "buckets", "cases" };
        private static readonly Regex Alias = new Regex(@"^(.+\))\s+AS\s+(\[[^\]]+\]|\w+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex Case = new Regex(@"^(.+)\s+AS\s+'((?:''|[^'])*)'\s*$", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static string Name(string expression)
        {
            var alias = Alias.Match(expression);

            return SqlSyntax.Unbracket(alias.Success ? alias.Groups[2].Value : expression);
        }

        public static IReadOnlyList<string> Labels(string expression)
        {
            Compile<object>(expression, null, true, out var labels);

            return labels;
        }

        public static Func<T, object> Compile<T>(string expression, Func<string, string> rewrite = null)
        {
            return Compile<T>(expression, rewrite ?? (text => text), false, out _);
        }

        private static Func<T, object> Compile<T>(string expression, Func<string, string> rewrite, bool labelsOnly, out IReadOnlyList<string> labels)
        {
            var alias = Alias.Match(expression);
            if (alias.Success)
            {
                expression = alias.Groups[1].Value;
            }

            labels = null;

            var calls = SqlSyntax.FindCalls(expression, BucketNames);
            if (calls.Count == 0)
            {
                return labelsOnly ? null : SqlExpression.Compile<T>(rewrite(expression));
            }
            if (!SqlSyntax.IsWholeCall(expression, calls))
            {
                throw new SqlException("FACET buckets() or cases() must be a complete facet expression.");
            }

            var args = SqlSyntax.SplitTopLevel(calls[0].Args);
            if (calls[0].Name.Equals("cases", StringComparison.OrdinalIgnoreCase))
            {
                var predicates = new Func<T, object>[args.Count];
                var caseLabels = new string[args.Count];

                for (var i = 0; i < args.Count; i++)
                {
                    var item = Case.Match(args[i]);
                    if (!item.Success)
                    {
                        throw new SqlException("cases takes conditions with labels, e.g. cases(RPM < 2000 AS 'Low', RPM >= 2000 AS 'High').");
                    }

                    caseLabels[i] = item.Groups[2].Value.Replace("''", "'");

                    if (!labelsOnly)
                    {
                        predicates[i] = SqlExpression.Compile<T>(rewrite(item.Groups[1].Value));
                    }
                }

                labels = caseLabels.Distinct().ToArray();

                return row =>
                {
                    for (var i = 0; i < predicates.Length; i++)
                    {
                        if (predicates[i](row) is true)
                        {
                            return caseLabels[i];
                        }
                    }
                    return null;
                };
            }

            if (args.Count != 3 ||
                !double.TryParse(args[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var ceiling) ||
                !double.IsFinite(ceiling) ||
                ceiling <= 0 ||
                !int.TryParse(args[2], NumberStyles.None, CultureInfo.InvariantCulture, out var count) ||
                count < 1 ||
                count > 1000 ||
                ceiling / count <= 0)
            {
                throw new SqlException("buckets takes (numeric expression, positive finite ceiling, bucket count from 1 to 1000).");
            }

            var value = labelsOnly ? null : SqlExpression.Compile<T>(rewrite(args[0]));
            var boundaries = new double[count - 1];
            var decimalWidth = ceiling < (double)decimal.MaxValue ? (decimal)ceiling / count : 0;
            var useDecimal = decimalWidth > 0 && (double)decimalWidth == ceiling / count;

            for (var i = 0; i < boundaries.Length; i++)
            {
                boundaries[i] = useDecimal ? (double)(decimalWidth * (i + 1)) : ceiling * ((i + 1d) / count);
            }

            var bucketLabels = new string[count];

            for (var i = 0; i < count; i++)
            {
                if (count == 1)
                {
                    bucketLabels[i] = "All values";
                }
                else if (i == 0)
                {
                    bucketLabels[i] = "< " + Format(boundaries[0]);
                }
                else if (i == count - 1)
                {
                    bucketLabels[i] = ">= " + Format(boundaries[i - 1]);
                }
                else
                {
                    bucketLabels[i] = Format(boundaries[i - 1]) + " - < " + Format(boundaries[i]);
                }
            }

            labels = bucketLabels;

            return row =>
            {
                var raw = value(row);
                if (raw == null)
                {
                    return null;
                }

                var code = Type.GetTypeCode(raw.GetType());
                if (raw.GetType().IsEnum || code < TypeCode.SByte || code > TypeCode.Decimal)
                {
                    throw new SqlException("buckets requires numeric values; received " + raw.GetType().Name + ".");
                }

                var number = Convert.ToDouble(raw, CultureInfo.InvariantCulture);
                if (!double.IsFinite(number))
                {
                    return null;
                }

                var index = Array.BinarySearch(boundaries, number);

                return bucketLabels[index < 0 ? ~index : index + 1];
            };
        }

        private static string Format(double number)
        {
            return number.ToString("G", CultureInfo.InvariantCulture);
        }
    }
}
