using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Expressions.Predicates.Boolean
{
    [SyntaxNode(RuleConstants.RULE_NOTEXP_NOT)]
    public class NotNode : ExpressionNode
    {
        public NotNode()
        {
        }

        internal override Expression CreateExpression(ParameterExpression sourceData, ParameterExpression param)
        {
            return Expression.Not(CreateChildExpression(sourceData, param, 0));
        }
    }
}
