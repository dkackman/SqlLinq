using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Expressions.Arithmetic
{
    [SyntaxNode(RuleConstants.RULE_NEGATEEXP_MINUS)]
    public class NegateNode : ExpressionNode
    {
        public NegateNode()
        {
        }

        internal override Expression CreateExpression(ParameterExpression sourceData, ParameterExpression param)
        {
            return Expression.NegateChecked(CreateChildExpression(sourceData, param, 1));
        }
    }
}
