using System;

using LinqStatistics;

namespace SqlLinq.SyntaxTree.Aggregates
{
    [SyntaxNode(RuleConstants.RULE_AGGREGATE_VAR_LPAREN_RPAREN)]
    public class VarianceNode : AggregateNode
    {
        public VarianceNode()
        {
            Name = "Variance";
        }

        protected override Type GetEvaluatatorType()
        {
            return typeof(EnumerableStats);
        }
    }
}
