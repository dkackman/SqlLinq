using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using SqlLinq.SyntaxTree;

namespace SqlLinq
{
    public class Query<TSource, TResult> : QueryBase<TResult>
    {
        public Query(string sql)
            : base(sql)
        {
            SourceFilters = new Filter<TSource>();
        }

        internal Filter<TSource> SourceFilters { get; private set; }

        internal Func<IEnumerable<TSource>, IEnumerable<TResult>> Select { get; private set; }

        protected override void OnCompile()
        {
            if (SyntaxNode.GroupByClause != null)
                Select = SyntaxNode.GroupByClause.CreateGroupBySelector<TSource, TResult>(SyntaxNode.Columns.Aggregates);

            else
                Select = SyntaxNode.Columns.CreateSelector<TSource, TResult>();

            // build up the source and result filters
            if (SyntaxNode.WhereClause != null)
                SourceFilters.Add(list => list.Where(SyntaxNode.WhereClause.CreateEvaluator<TSource>()));

            if (SyntaxNode.OrderByClause != null)
                SourceFilters.Add(SyntaxNode.OrderByClause.CreateEvaluator<TSource>());

            if (SyntaxNode.HavingClause != null)
                ResultFilters.Add(list => list.Where(SyntaxNode.HavingClause.CreateEvaluator<TResult>()));

            if (SyntaxNode.Columns.Distinct)
                ResultFilters.Add(list => list.Distinct(CreateDistinctComparer()));
        }

        public IEnumerable<TResult> Evaluate(IEnumerable<TSource> source)
        {
            Debug.Assert(source != null);
            Debug.Assert(Select != null);

            var result = Select(SourceFilters.Evaluate(source));

            return ResultFilters.Evaluate(result);
        }
    }
}
