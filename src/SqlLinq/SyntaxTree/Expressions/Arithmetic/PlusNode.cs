using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Expressions.Arithmetic
{
    [SyntaxNode(RuleConstants.RULE_ADDEXP_PLUS)]
    public class PlusNode : BinaryExpressionNode
    {
        public PlusNode()
        {
        }

        protected override Expression CreateChildExpression(Expression left, Expression right)
        {
            return Expression.AddChecked(left, right);
        }
    }
}
