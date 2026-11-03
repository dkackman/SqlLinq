using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnitsNet;
using UnitsNet.Metadata;

namespace SqlLinq.Units
{
    public sealed class UnitConversions
    {
        private static readonly string[] Names = { "convert" };
        private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");

        private readonly List<QuantityMetadata> _metadata;

        public UnitConversions(IEnumerable<QuantityMetadata> metadata)
        {
            _metadata = metadata.Where(m => m.Unit != null).ToList();
        }

        public string Expand(string text)
        {
            var calls = SqlSyntax.FindCalls(text, Names);

            return SqlSyntax.ReplaceCalls(text, calls, i => Rewrite(calls[i]));
        }

        private string Rewrite(FunctionCall call)
        {
            var args = SqlSyntax.SplitTopLevel(call.Args);

            if (args.Count != 2 && args.Count != 3)
            {
                throw new SqlException("convert takes (column, 'to unit') or (value, 'Kind.from unit', 'to unit').");
            }

            var fromText = args.Count == 3 ? SqlSyntax.Unquote(args[1]) : null;
            var toText = SqlSyntax.Unquote(args[args.Count - 1]);
            var column = _metadata.FirstOrDefault(m => m.Property.Name.Equals(SqlSyntax.Unbracket(args[0]), StringComparison.OrdinalIgnoreCase));

            var convert = column != null
                ? ColumnConversion(column, fromText, toText)
                : KindConversion(args[0], fromText, toText);

            var offset = 0d;
            var scale = 0d;
            try
            {
                offset = convert(0);
                scale = convert(1) - offset;
                foreach (var x in new[] { -40.0, 7.0, 1000.0 })
                {
                    var expected = scale * x + offset;
                    if (Math.Abs(convert(x) - expected) > 1e-9 * Math.Max(1, Math.Abs(expected)))
                    {
                        throw new NotSupportedException();
                    }
                }
            }
            catch (Exception e)
            {
                throw new SqlException(string.Format(CultureInfo.CurrentCulture, "{0} isn't a linear conversion, which convert doesn't support.", call.Args), e);
            }

            return string.Format(CultureInfo.InvariantCulture, "(({0}) * {1} + {2})", Expand(args[0]), Literal(scale), Literal(offset));
        }

        private static Func<double, double> ColumnConversion(QuantityMetadata column, string fromText, string toText)
        {
            var unit = column.Unit;

            if (fromText != null && !Equals(Choice(column, fromText), unit.UnitInfo.Value))
            {
                throw new SqlException(string.Format(CultureInfo.CurrentCulture, "{0} is in {1}, not {2}.", column.Property.Name, unit.Name, fromText));
            }

            var to = Choice(column, toText);

            if (to.Equals(unit.UnitInfo.Value))
            {
                return x => x;
            }

            var allowed = column.Conversions.First(c => c.UnitInfo.Value.Equals(to));
            var target = UnitMetadata.FromUnitInfo(allowed.UnitInfo, allowed.QuantityType.QuantityInfo);

            return x => (double)(unit.TryConvertQuantity(x, target, out var converted) ? converted : converted.ToUnit(to)).Value;
        }

        private static Enum Choice(QuantityMetadata column, string text)
        {
            var kind = column.Unit.QuantityType.QuantityInfo.Name + ".";
            var unit = Resolve(column, text) ?? (text.StartsWith(kind, StringComparison.OrdinalIgnoreCase) ? Resolve(column, text[kind.Length..]) : null);

            return unit ?? throw new SqlException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    "{0} ({1}) can't convert to '{2}'. Its units are {3}.",
                    column.Property.Name,
                    column.Unit.Name,
                    text,
                    string.Join(", ", UnitChoices.For(column).Select(c => c.Symbol.Length > 0 ? c.Symbol : c.Name))));
        }

        private static Enum Resolve(QuantityMetadata column, string text)
        {
            try
            {
                return UnitChoices.Resolve(column, text);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private static Func<double, double> KindConversion(string value, string fromText, string toText)
        {
            if (fromText == null)
            {
                throw new SqlException(string.Format(CultureInfo.CurrentCulture, "convert({0}, '{1}') needs a column with unit metadata. Otherwise name the from unit and its kind, e.g. convert({0}, 'Pressure.kPa', '{1}').", value, toText));
            }

            if (!SplitKind(fromText, out var kind, out var fromUnit))
            {
                var kinds = Quantity.Infos.Where(i => Find(fromText, i) != null).Select(i => string.Format(CultureInfo.CurrentCulture, "'{0}.{1}'", i.Name, fromText)).ToList();

                throw new SqlException(
                    kinds.Count == 0
                        ? string.Format(CultureInfo.CurrentCulture, "'{0}' isn't a unit of any kind.", fromText)
                        : string.Format(CultureInfo.CurrentCulture, "{0} has no unit metadata, so name the kind of '{1}': {2}.", value, fromText, string.Join(" or ", kinds)));
            }

            var from = Find(fromUnit, kind) ?? throw new SqlException(string.Format(CultureInfo.CurrentCulture, "'{0}' isn't a {1} unit.", fromUnit, kind.Name));
            var toUnit = toText;

            if (SplitKind(toText, out var toKind, out var unqualified))
            {
                if (toKind != kind)
                {
                    throw new SqlException(string.Format(CultureInfo.CurrentCulture, "'{0}' is {1}, not {2}.", fromText, kind.Name, toKind.Name));
                }
                toUnit = unqualified;
            }

            var to = Find(toUnit, kind) ?? throw new SqlException(string.Format(CultureInfo.CurrentCulture, "'{0}' isn't a {1} unit.", toText, kind.Name));

            return x => UnitConverter.Convert(x, from, to);
        }

        private static bool SplitKind(string text, out QuantityInfo kind, out string unit)
        {
            var dot = text.IndexOf('.');

            kind = dot > 0 ? Quantity.Infos.FirstOrDefault(i => i.Name.Equals(text[..dot], StringComparison.OrdinalIgnoreCase)) : null;
            unit = kind != null ? text[(dot + 1)..] : null;

            return kind != null;
        }

        private static Enum Find(string text, QuantityInfo kind)
        {
            foreach (var unit in kind.UnitInfos)
            {
                if (unit.Name.Equals(text, StringComparison.OrdinalIgnoreCase)
                    || UnitAbbreviationsCache.Default.GetUnitAbbreviations(kind.UnitType, Convert.ToInt32(unit.Value, CultureInfo.InvariantCulture), English).Contains(text))
                {
                    return unit.Value;
                }
            }

            return null;
        }

        private static string Literal(double value)
        {
            var digits = Math.Abs(value).ToString("0.0###########################", CultureInfo.InvariantCulture);

            return value < 0 ? "(-" + digits + ")" : digits;
        }
    }
}
