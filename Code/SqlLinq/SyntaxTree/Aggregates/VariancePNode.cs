using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlLinq.SyntaxTree.Aggregates
{
    [SyntaxNode(RuleConstants.RULE_AGGREGATE_VARP_LPAREN_RPAREN)]
    public class VariancePNode : VarianceNode
    {
        public VariancePNode()
        {
            Name = "VarianceP";
        }
    }
}
