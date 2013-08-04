using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Expressions.Arithmetic
{
    [SyntaxNode(RuleConstants.RULE_MULTEXP_DIV)]
    public class DivideNode : BinaryExpressionNode
    {
        public DivideNode()
        {
        }

        protected override Expression CreateChildExpression(Expression left, Expression right)
        {
            return Expression.Divide(left, right);
        }
    }
}
 