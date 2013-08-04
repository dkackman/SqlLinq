using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlLinq.SyntaxTree.Joins
{
    [SyntaxNode(RuleConstants.RULE_JOIN_JOIN_ON_ID_EQ_ID)]
    [SyntaxNode(RuleConstants.RULE_JOIN_INNER_JOIN_ON_ID_EQ_ID)]
    public class Inner : Join
    {
        public Inner()
        {
        }
    }
}
