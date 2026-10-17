using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Expressions.Predicates.Comparison
{
    [SyntaxNode(RuleConstants.RULE_PREDEXP_IS_NOT_NULL)]
    public class IsNotNullNode : IsNullNode
    {
        public IsNotNullNode()
        {
        }

        internal override Expression CreateExpression(ParameterExpression sourceData, ParameterExpression param)
        {
            return Expression.Not(base.CreateExpression(sourceData, param));
        }
    }
}
