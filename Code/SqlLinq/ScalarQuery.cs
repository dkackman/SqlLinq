using System;
using System.Collections.Generic;

using SqlLinq.SyntaxTree;

namespace SqlLinq
{
    public class ScalarQuery<TSource, TResult> : QueryBase<TResult>
    {
        private Query<TSource, object> _query;

        public ScalarQuery(string sql)
            : base(sql)
        {
            _query = new Query<TSource, object>(sql);
        }

        public Func<IEnumerable<TSource>, TResult> Select { get; private set; }

        protected override void OnCompile()
        {
            if (Sql == "subquery") // this whole subquery bit is still a work in progress
                ((ICompile)_query).Compile(SyntaxNode);
            else
                _query.Compile();

            Select = SyntaxNode.Columns.CreateScalarSelector<TSource, TResult>(_query.Evaluate);
        }

        public TResult Evaluate(IEnumerable<TSource> source)
        {
            return Select(source);
        }
    }
}
