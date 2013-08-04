using System.Diagnostics;
using System.Reflection;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

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
            MethodInfo match = typeof(LikeNode).GetMethod("IsSqlLikeMatch");
            Expression pattern = Expression.Constant(GetTerminalText("StringLiteral").Trim('\''));

            return Expression.Call(match, CreateChildExpression(sourceData, param, 0), pattern);
        }

        public static bool IsSqlLikeMatch(string input, string pattern)
        {
            if (input == null)
                return false;

            /* Turn "off" all regular expression related syntax in the pattern string. */
            pattern = Regex.Escape(pattern);

            /* Replace the SQL LIKE wildcard metacharacters with the
            * equivalent regular expression metacharacters. */
            pattern = pattern.Replace("%", ".*?").Replace("_", ".");

            /* The previous call to Regex.Escape actually turned off
            * too many metacharacters, i.e. those which are recognized by
            * both the regular expression engine and the SQL LIKE
            * statement ([...] and [^...]). Those metacharacters have
            * to be manually unescaped here. */
            pattern = pattern.Replace(@"\[", "[").Replace(@"\]", "]").Replace(@"\^", "^");

            return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
        }
    }
}
