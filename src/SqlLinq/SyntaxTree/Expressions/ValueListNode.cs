using System.Linq.Expressions;
using System.Linq;

using SqlLinq.SyntaxTree.Expressions.Literals;

namespace SqlLinq.SyntaxTree.Expressions
{
    [SyntaxNode(RuleConstants.RULE_VALUELIST_COMMA)]
    public class ValueListNode : ExpressionNode
    {
        public ValueListNode()
        {

        }

        internal override Expression CreateExpression(ParameterExpression sourceData, ParameterExpression param)
        {
            var type = FindDescendants<LiteralNode>().First().Type; // the first literal defines the type of the list

            return Expression.NewArrayInit(type, FindDescendants<LiteralNode>().Select(l => l.CreateExpression(sourceData, param)));
        }
    }
}
