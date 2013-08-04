using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SqlLinq.SyntaxTree.Aggregates
{
    [SyntaxNode(RuleConstants.RULE_AGGREGATE_COUNT_LPAREN_DISTINCT_RPAREN)]
    class CountDistinctNode : CountNode
    {
        public CountDistinctNode()
        {
            Name = "CountDistinct";
        }

        public static int CountDistinct<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return CountDistinct<TResult>(source.Select(selector));
        }

        public static int CountDistinct<TSource>(IEnumerable<TSource> source)
        {
            // the behavior of the implementation of COUNT(DISTINCT) ignores nulls to match how SQL works
            // http://msdn.microsoft.com/en-us/library/ms175997.aspx
            //
            return source.Where<TSource>(t => t != null).Distinct<TSource>().Count<TSource>();
        }
    }
}
