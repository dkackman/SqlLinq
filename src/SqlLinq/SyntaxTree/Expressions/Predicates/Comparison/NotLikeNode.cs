using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Expressions.Predicates.Comparison
{
    [SyntaxNode(RuleConstants.RULE_PREDEXP_NOT_LIKE_STRINGLITERAL)]
    public class NotLikeNode : LikeNode
    {
        public NotLikeNode()
        {
        }

        internal override Expression CreateExpression(ParameterExpression sourceData, ParameterExpression param)
        {
            return Expression.Not(base.CreateExpression(sourceData, param));
        }
    }
}
