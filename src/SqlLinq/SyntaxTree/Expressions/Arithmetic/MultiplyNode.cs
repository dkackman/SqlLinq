using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Expressions.Arithmetic
{
    [SyntaxNode(RuleConstants.RULE_MULTEXP_TIMES)]
    public class MultiplyNode : ArithmeticExpressionNode
    {
        public MultiplyNode()
        {
        }

        protected override Expression CreateChildExpression(Expression left, Expression right)
        {
            return Expression.MultiplyChecked(left, right);
        }
    }
}
