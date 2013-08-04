using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlLinq.SyntaxTree.Aggregates
{
    [SyntaxNode(RuleConstants.RULE_AGGREGATE_STDEVP_LPAREN_RPAREN)]
    public class StandardDeviationPNode : VarianceNode
    {
        public StandardDeviationPNode()
        {
            Name = "StandardDeviationP";
        }
    }
}
