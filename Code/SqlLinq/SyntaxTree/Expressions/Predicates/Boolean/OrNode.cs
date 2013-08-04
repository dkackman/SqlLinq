using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Expressions.Predicates.Boolean
{
    [SyntaxNode(RuleConstants.RULE_EXPRESSION_OR)]
    public class OrNode : BinaryExpressionNode
    {
        public OrNode()
        {
        }

        protected override Expression CreateChildExpression(Expression left, Expression right)
        {
            return Expression.OrElse(left, right);
        }
    }
}
