using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Expressions.Predicates.Boolean
{
    [SyntaxNode(RuleConstants.RULE_ANDEXP_AND)]
    public class AndNode : BinaryExpressionNode
    {
        public AndNode()
        {
        }

        protected override Expression CreateChildExpression(Expression left, Expression right)
        {
            return Expression.AndAlso(left, right);
        }
    }
}
