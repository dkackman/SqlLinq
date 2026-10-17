using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlLinq.SyntaxTree.Joins
{
    [SyntaxNode(RuleConstants.RULE_JOIN_LEFT_JOIN_ON_ID_EQ_ID)]
    public class Left : Join
    {
        public Left()
        {
        }
    }
}
