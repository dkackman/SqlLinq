﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlLinq.SyntaxTree.Aggregates
{
    [SyntaxNode(RuleConstants.RULE_AGGREGATE_AVG_LPAREN_RPAREN)]
    public class AverageNode : AggregateNode
    {
        public AverageNode()
        {
            Name = "Average";
        }
    }
}
