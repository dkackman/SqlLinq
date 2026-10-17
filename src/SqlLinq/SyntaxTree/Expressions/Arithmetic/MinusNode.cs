using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Expressions.Arithmetic
{
    [SyntaxNode(RuleConstants.RULE_ADDEXP_MINUS)]
    public class MinusNode : BinaryExpressionNode
    {
        public MinusNode()
        {
        }

        protected override Expression CreateChildExpression(Expression left, Expression right)
        {
            return Expression.SubtractChecked(left, right);
        }
    }
}
