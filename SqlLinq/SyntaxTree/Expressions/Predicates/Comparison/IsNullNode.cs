using System;
using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Expressions.Predicates.Comparison
{
    [SyntaxNode(RuleConstants.RULE_PREDEXP_IS_NULL)]
    public class IsNullNode : ExpressionNode
    {
        public IsNullNode()
        {
        }

        internal override Expression CreateExpression(ParameterExpression sourceData, ParameterExpression param)
        {
            return Expression.Equal(CreateChildExpression(sourceData, param, 0), Expression.Constant(null));
        }
    }
}
