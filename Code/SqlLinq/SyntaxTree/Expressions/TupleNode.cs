using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Expressions
{
    [SyntaxNode(RuleConstants.RULE_TUPLE_LPAREN_RPAREN2)]
    public class TupleNode : ExpressionNode
    {
        public TupleNode()
        {
        }

        internal override Expression CreateExpression(ParameterExpression sourceData, ParameterExpression param)
        {
            return CreateChildExpression(sourceData, param, 1); // a tuple will of the form '(' '<expression>' ')' so return the expression of the middle node
        }
    }
}
