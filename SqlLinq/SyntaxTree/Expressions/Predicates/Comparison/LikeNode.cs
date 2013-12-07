using System.Diagnostics;
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
            StringBuilder builder = new StringBuilder(Regex.Escape(pattern));

            // these are needed because the .*? replacement below at the begining or end of the string is not
            // accounting for cases such as LIKE '%abc' or LIKE 'abc%'
            bool startsWith = pattern.StartsWith("%") && !pattern.EndsWith("%");
            bool endsWith = !pattern.StartsWith("%") && pattern.EndsWith("%");

            // this is a little tricky
            // ends with in like is '%abc'
            // in regex it's 'abc$'
            // so need to tanspose
            if (startsWith)
            {
                builder.Replace("%", "", 0, 1);
                builder.Append("$");
            }

            // same but inverse here
            if (endsWith)
            {
                builder.Replace("%", "", pattern.Length - 1, 1);
                builder.Insert(0, "^");
            }

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
