using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Expressions.Predicates.Comparison
{
    [SyntaxNode(RuleConstants.RULE_PREDEXP_NOT_BETWEEN_AND)]
    public class NotBetweenNode : BetweenNode
    {
        public NotBetweenNode()
        {
        }

        internal override Expression CreateExpression(ParameterExpression sourceData, ParameterExpression param)
        {
            return Expression.Not(CreateBetweenExpression(sourceData, param, CreateChildExpression(sourceData, param, 3), CreateChildExpression(sourceData, param, 5)));
        }
    }
}
