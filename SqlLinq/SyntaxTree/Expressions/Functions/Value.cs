using System;
using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Expressions.Functions
{
    [SyntaxNode(RuleConstants.RULE_VALUE_FUNCTION)]
    public class Value : FunctionNode
    {
        public Value()
        {
        }

        internal override Expression CreateExpression(ParameterExpression sourceData, ParameterExpression param)
        {
            return Expression.Convert(param, param.Type);
        }
    }
}
