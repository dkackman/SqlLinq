using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Expressions.Predicates.Comparison
{
    [SyntaxNode(RuleConstants.RULE_PREDEXP_GTEQ)]
    public class GreaterThanOrEqualNode : PredicateExpressionNode
    {
        public GreaterThanOrEqualNode()
        {
        }

        protected override Expression CreateChildExpression(Expression left, Expression right)
        {
            return Expression.GreaterThanOrEqual(left, right);
        }
    }
}
