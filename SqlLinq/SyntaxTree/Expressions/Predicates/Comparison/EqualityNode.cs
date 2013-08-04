using System;
using System.Reflection;
using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Expressions.Predicates.Comparison
{
    [SyntaxNode(RuleConstants.RULE_PREDEXP_EQ)]
    public class EqualityNode : PredicateExpressionNode
    {
        public EqualityNode()
        {
        }

        protected override Expression CreateChildExpression(Expression left, Expression right)
        {
            return Expression.Equal(left, right);
        }
    }
}
