using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Expressions.Arithmetic
{
    [SyntaxNode(RuleConstants.RULE_NEGATEEXP_MINUS)]
    public class NegateNode : ExpressionNode
    {
        internal override Expression CreateExpression(ParameterExpression sourceData, ParameterExpression param)
        {
            var value = CreateChildExpression(sourceData, param, 1);
            if (value.Type != typeof(object))
            {
                return Expression.NegateChecked(value);
            }

            return Expression.Convert(Expression.NegateChecked(ArithmeticExpressionNode.ToNumber(value)), typeof(object));
        }
    }
}
