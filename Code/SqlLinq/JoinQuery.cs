using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using SqlLinq.SyntaxTree;
using SqlLinq.SyntaxTree.Joins;

namespace SqlLinq
{
    public class JoinQuery<TSource, TInner, TResult> : QueryBase<TResult>
    {
        public JoinQuery(string sql)
            : base(sql)
        {
        }

        private Func<IEnumerable<TSource>, IEnumerable<TInner>, IEnumerable<JoinResultSelector<TSource, TInner>>> _joinFunction;
        private Filter<JoinResultSelector<TSource, TInner>> _joinFilters = new Filter<JoinResultSelector<TSource, TInner>>();
        private Func<IEnumerable<JoinResultSelector<TSource, TInner>>, IEnumerable<TResult>> _resultSelector;

        protected override void OnCompile()
        {
            if (SyntaxNode.FromClause.Join == null)
                throw new SqlException("The query does not contain a JOIN expression\n" + Sql);

            _joinFunction = SyntaxNode.FromClause.Join.CreateJoinFunction<TSource, TInner>();

            if (SyntaxNode.GroupByClause != null)
                _resultSelector = SyntaxNode.GroupByClause.CreateGroupBySelector<JoinResultSelector<TSource, TInner>, TResult>(SyntaxNode.Columns.Aggregates);
            else
                _resultSelector = SyntaxNode.Columns.CreateSelector<JoinResultSelector<TSource, TInner>, TResult>();

            if (SyntaxNode.WhereClause != null)
                _joinFilters.Add(list => list.Where(SyntaxNode.WhereClause.CreateEvaluator<JoinResultSelector<TSource, TInner>>()));

            if (SyntaxNode.OrderByClause != null)
                _joinFilters.Add(SyntaxNode.OrderByClause.CreateEvaluator<JoinResultSelector<TSource, TInner>>());

            if (SyntaxNode.HavingClause != null)
                ResultFilters.Add(list => list.Where(SyntaxNode.HavingClause.CreateEvaluator<TResult>()));

            if (SyntaxNode.Columns.Distinct)
                ResultFilters.Add(list => list.Distinct(CreateDistinctComparer()));
        }

        public IEnumerable<TResult> Evaluate(IEnumerable<TSource> source, IEnumerable<TInner> inner)
        {
            Debug.Assert(source != null);
            Debug.Assert(inner != null);

            var join = _joinFunction(source, inner); // the joined data
            var filtered = _joinFilters.Evaluate(join); // joined data filtered and ordered
            var result = _resultSelector(filtered); // the results (trasnformed)

            return ResultFilters.Evaluate(result); // the results filtered and ordered
        }
    }
}
