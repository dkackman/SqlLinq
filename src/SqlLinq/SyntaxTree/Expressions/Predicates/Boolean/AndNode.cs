using System.Linq.Expressions;

#pragma warning disable CA1716
namespace SqlLinq.SyntaxTree.Expressions.Predicates.Boolean
#pragma warning restore CA1716
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
