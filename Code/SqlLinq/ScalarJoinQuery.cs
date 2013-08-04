using System;
using System.Collections.Generic;

using SqlLinq.SyntaxTree;

namespace SqlLinq
{
    public class ScalarJoinQuery<TSource, TInner, TResult> : QueryBase<TResult>
    {
        private JoinQuery<TSource, TInner, object> _query;

        public ScalarJoinQuery(string sql)
            : base(sql)
        {
            _query = new JoinQuery<TSource, TInner, object>(sql);
        }

        internal Func<IEnumerable<TSource>, IEnumerable<TInner>, TResult> Select { get; private set; }

        protected override void OnCompile()
        {
            if (Sql == "subquery") // this whole subquery bit is still a work in progress
                ((ICompile)_query).Compile(SyntaxNode);
            else
                _query.Compile();

            Select = SyntaxNode.Columns.CreateScalarSelector<TSource, TInner, TResult>(_query.Evaluate);
        }

        public TResult Evaluate(IEnumerable<TSource> source, IEnumerable<TInner> inner)
        {
            return Select(source, inner);
        }
    }
}
