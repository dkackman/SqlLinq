using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SqlLinq.SyntaxTree.Aggregates
{
    [SyntaxNode(RuleConstants.RULE_AGGREGATE_COUNT_LPAREN_ALL_RPAREN)]
    public class CountAllNode : CountNode
    {
        public CountAllNode()
        {
            Name = "CountAll";
        }

        public static int CountAll<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return CountAll<TResult>(source.Select(selector));
        }

        public static int CountAll<TSource>(IEnumerable<TSource> source)
        {
            return source.Where<TSource>(t => t != null).Count<TSource>();
        }
    }
}
