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
            MethodInfo match = typeof(LikeNode).GetMethod("IsRegexMatch");
            
            string like = GetTerminalText("StringLiteral").Trim('\'');
            string regex = ConvertLikeToRegex(like);

            Expression pattern = Expression.Constant(regex);

            return Expression.Call(match, CreateChildExpression(sourceData, param, 0), pattern);
        }        

        public static bool IsRegexMatch(string input, string regex)
        {
            if (input == null)
                return false;

            return Regex.IsMatch(input, regex, RegexOptions.IgnoreCase);
        }

        internal static string ConvertLikeToRegex(string pattern)
        {
            /* Turn "off" all regular expression related syntax in the pattern string. */
            StringBuilder builder = new StringBuilder();
            builder.Append("^");
            builder.Append(Regex.Escape(pattern));
            builder.Append("$");                

            /* Replace the SQL LIKE wildcard metacharacters with the
            * equivalent regular expression metacharacters. */
            builder.Replace("%", ".*?").Replace("_", ".");

            /* The previous call to Regex.Escape actually turned off
            * too many metacharacters, i.e. those which are recognized by
            * both the regular expression engine and the SQL LIKE
            * statement ([...] and [^...]). Those metacharacters have
            * to be manually unescaped here. */
            builder.Replace(@"\[", "[").Replace(@"\]", "]").Replace(@"\^", "^");

            return builder.ToString();
        }
    }
}
