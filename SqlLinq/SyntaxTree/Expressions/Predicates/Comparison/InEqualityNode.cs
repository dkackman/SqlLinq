using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Expressions.Predicates.Comparison
{
    [SyntaxNode(RuleConstants.RULE_PREDEXP_EXCLAMEQ)]
    [SyntaxNode(RuleConstants.RULE_PREDEXP_LTGT)]
    public class InequalityNode : PredicateExpressionNode
    {
        public InequalityNode()
        {
        }

        protected override Expression CreateChildExpression(Expression left, Expression right)
        {
            return Expression.NotEqual(left, right);
        }
    }
}
