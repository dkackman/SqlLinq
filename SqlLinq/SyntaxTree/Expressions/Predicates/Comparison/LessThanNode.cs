using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Expressions.Predicates.Comparison
{
    [SyntaxNode(RuleConstants.RULE_PREDEXP_LT)]
    public class LessThanNode : PredicateExpressionNode
    {
        public LessThanNode()
        {
        }

        protected override Expression CreateChildExpression(Expression left, Expression right)
        {
            return Expression.LessThan(left, right);
        }
    }
}
