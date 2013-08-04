using System;
using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Expressions
{
    [SyntaxNode(RuleConstants.RULE_IDMEMBER_ID)]
    [SyntaxNode(RuleConstants.RULE_VALUE_ID)]
    [SyntaxNode(RuleConstants.RULE_COLUMNALIAS_AS_ID)]
    public class NodeWithId : ExpressionNode
    {
        private Lazy<Identifier> _id;

        public NodeWithId()
        {
            _id = new Lazy<Identifier>(() => new Identifier(GetTerminalText("Id").Trim('[', ']')));
        }

        public Identifier Id
        {
            get
            {
                return _id.Value;
            }
        }

        internal override Expression CreateExpression(ParameterExpression sourceData, ParameterExpression param)
        {
            return ExpressionFactory.CreateFieldSelector(param, Id.LookupId);
        }
    }
}
