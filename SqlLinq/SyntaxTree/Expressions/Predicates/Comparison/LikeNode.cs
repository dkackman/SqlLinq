using System;
using System.Reflection;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Text;

namespace SqlLinq.SyntaxTree.Expressions.Predicates.Comparison
{
    [SyntaxNode(RuleConstants.RULE_PREDEXP_LIKE_STRINGLITERAL)]
    public class LikeNode : ExpressionNode
    {
        public LikeNode()
        {
        }

        internal override Expression CreateExpression(ParameterExpression sourceData, ParameterExpression param)
        {
            MethodInfo matchMethod = typeof(LikeNode).GetMethod("IsRegexMatch", new Type[] { typeof(object), typeof(Regex) });

            string like = GetTerminalText("StringLiteral").Trim('\'');
            var regex = new Regex(ConvertLikeToRegex(like), RegexOptions.IgnoreCase);

            Expression match = Expression.Constant(regex);

            return Expression.Call(matchMethod, CreateChildExpression(sourceData, param, 0), match);
        }

        public static bool IsRegexMatch(object input, Regex regex)
        {
            if (input == null)
                return false;

            return regex.IsMatch(input.ToString());
        }

        internal static bool IsRegexMatch(string input, string regex)
        {
            if (input == null)
                return false;

            return Regex.IsMatch(input, regex, RegexOptions.IgnoreCase);
        }

        internal static string ConvertLikeToRegex(string pattern)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("^");
            builder.Append(Regex.Escape(pattern)); // Turn "off" all regular expression related syntax in the pattern string
            builder.Append("$");

            /* Replace the SQL LIKE wildcard metacharacters with the
            * equivalent regular expression metacharacters. */
            builder.Replace("%", ".*").Replace("_", ".");

            /* The previous call to Regex.Escape actually turned off
            * too many metacharacters, i.e. those which are recognized by
            * both the regular expression engine and the SQL LIKE
            * statement ([...] and [^...]). Those metacharacters have
            * to be manually unescaped here. */
            builder.Replace(@"\[", "[").Replace(@"\]", "]").Replace(@"\^", "^");

            // put like syntax wildcard literals back
            builder.Replace("[.*]", "[%]").Replace("[.]", "[_]");

            return builder.ToString();
        }
    }
}
