using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Diagnostics;

using SqlLinq.SyntaxTree.Aggregates;

namespace SqlLinq.SyntaxTree
{
    [SyntaxNode(RuleConstants.RULE_COLUMNS_TIMES)]
    public class ColumnsStar : Columns
    {
        public ColumnsStar()
        {
        }

        public override Func<IEnumerable<TSource>, IEnumerable<TResult>> CreateSelector<TSource, TResult>()
        {
            return list => list.Select(ExpressionFactory.CreateIdentitySelector<TSource, TResult>().Compile());
        }
    }
}
