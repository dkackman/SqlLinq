using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlLinq
{
    public static class SqlSyntax
    {
        private static readonly Regex CallStart = new Regex(@"'[^']*'|\[[^\]]*\]|\b(\w+)\s*\(");

        public static List<string> SplitTopLevel(string text)
        {
            var parts = new List<string>();
            var depth = 0;
            var start = 0;
            var quoted = false;
            var bracketed = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (quoted)
                {
                    quoted = c != '\'';
                }
                else if (bracketed)
                {
                    bracketed = c != ']';
                }
                else if (c == '\'')
                {
                    quoted = true;
                }
                else if (c == '[')
                {
                    bracketed = true;
                }
                else if (c == '(')
                {
                    depth++;
                }
                else if (c == ')')
                {
                    depth--;
                }
                else if (c == ',' && depth == 0)
                {
                    parts.Add(text[start..i].Trim());
                    start = i + 1;
                }
            }

            parts.Add(text[start..].Trim());

            return parts;
        }

        public static List<FunctionCall> FindCalls(string text, ICollection<string> names)
        {
            var calls = new List<FunctionCall>();
            var i = 0;

            Match match;

            while ((match = CallStart.Match(text, i)).Success)
            {
                if (!match.Groups[1].Success || !names.Contains(match.Groups[1].Value))
                {
                    i = match.Index + match.Length;
                    continue;
                }

                var open = match.Index + match.Length - 1;
                var close = ClosingParen(text, open);

                if (close < 0)
                {
                    throw new SqlException("Unbalanced parentheses in " + text);
                }
                calls.Add(new FunctionCall(match.Index, close, match.Groups[1].Value, text.Substring(open + 1, close - open - 1)));

                i = close + 1;
            }

            return calls;
        }

        public static string ReplaceCalls(string text, List<FunctionCall> calls, Func<int, string> replacement)
        {
            var sb = new StringBuilder();
            var i = 0;

            for (var n = 0; n < calls.Count; n++)
            {
                sb
                    .Append(text, i, calls[n].Start - i)
                    .Append(replacement(n));

                i = calls[n].End + 1;
            }

            return sb.Append(text, i, text.Length - i).ToString();
        }

        public static bool IsWholeCall(string text, List<FunctionCall> calls)
        {
            return calls.Count == 1 && calls[0].Start == 0 && calls[0].End == text.Length - 1;
        }

        public static string Unbracket(string text)
        {
            return text.Length > 1 && text[0] == '[' && text[text.Length - 1] == ']' ? text[1..^1] : text;
        }

        public static string Unquote(string text)
        {
            if (text.Length < 2 || text[0] != '\'' || text[text.Length - 1] != '\'')
            {
                throw new SqlException("Expected a quoted string but found " + text);
            }

            return text[1..^1];
        }

        private static int ClosingParen(string text, int open)
        {
            var depth = 0;
            var quoted = false;
            var bracketed = false;

            for (var i = open; i < text.Length; i++)
            {
                var c = text[i];
                if (quoted)
                {
                    quoted = c != '\'';
                }
                else if (bracketed)
                {
                    bracketed = c != ']';
                }
                else if (c == '\'')
                {
                    quoted = true;
                }
                else if (c == '[')
                {
                    bracketed = true;
                }
                else if (c == '(')
                {
                    depth++;
                }
                else if (c == ')' && --depth == 0)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
