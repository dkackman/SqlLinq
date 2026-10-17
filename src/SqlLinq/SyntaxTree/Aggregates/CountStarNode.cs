using System;
using System.Linq;

namespace SqlLinq.SyntaxTree.Aggregates
{
    [SyntaxNode(RuleConstants.RULE_AGGREGATE_COUNT_LPAREN_TIMES_RPAREN)]
    public class CountStarNode : CountNode
    {
        public CountStarNode()
        {
            Name = "Count";
        }

        protected override Identifier CreateIdentifier()
        {
            return new Identifier(Name + "Star");
        }

        public override bool DoNotDereferenceFields
        {
            get
            {
                return true;
            }
        }

        protected override Type EvaluatatorType
        {
            get
            {
                return typeof(Enumerable);
            }
        }
    }
}
