using System;
using System.Linq;
using System.Reflection;

using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Expressions.Predicates.Comparison
{
    [SyntaxNode(RuleConstants.RULE_PREDEXP_NOT_IN)]
    public class NotInNode : InNode
    {
        public NotInNode()
        {
        }

        internal override Expression CreateExpression(ParameterExpression sourceData, ParameterExpression param)
        {
            return Expression.Not(CreateInExpression(CreateChildExpression(sourceData, param, 3), CreateChildExpression(sourceData, param, 0)));
        }
    }
}
