using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Expressions.Predicates.Comparison
{
    [SyntaxNode(RuleConstants.RULE_PREDEXP_LTEQ)]
    public class LessThanOrEqualNode : PredicateExpressionNode
    {
        public LessThanOrEqualNode()
        {
        }

        protected override Expression CreateChildExpression(Expression left, Expression right)
        {
            return Expression.LessThanOrEqual(left, right);
        }
    }
}
