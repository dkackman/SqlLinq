using System.Linq.Expressions;
using System.Linq;

using SqlLinq.SyntaxTree.Expressions.Literals;

namespace SqlLinq.SyntaxTree.Expressions
{
    [SyntaxNode(RuleConstants.RULE_INLIST_LPAREN_RPAREN)]
    public class InListNode : ExpressionNode
    {
        public InListNode()
        {

        }

        internal override Expression CreateExpression(ParameterExpression sourceData, ParameterExpression param)
        {
            return CreateChildExpression(sourceData, param, 1);
        }
    }
}
