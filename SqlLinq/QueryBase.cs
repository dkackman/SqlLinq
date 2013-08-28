using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using SqlLinq.SyntaxTree;

namespace SqlLinq
{
    internal interface ICompile
    {
        void Compile(SelectNode syntaxNode);
    }

    public abstract class QueryBase<TResult> : ICompile
    {
        protected QueryBase(string sql)
        {
            Debug.Assert(!string.IsNullOrEmpty(sql));
            Sql = sql;
            ResultFilters = new Filter<TResult>();
        }

        void ICompile.Compile(SelectNode syntaxNode)
        {
            SyntaxNode = syntaxNode;
            OnCompile();
        }

        public void Compile()
        {
            SqlParser parser = new SqlParser();
            if (!parser.Parse(Sql))
                throw new SqlException(string.Format("SQL parse error:\n\t{0}\nin statement\n\t{1}", parser.ErrorString, parser.ErrorLine));

            SyntaxNode = parser.SyntaxTree as SelectNode;
            Debug.Assert(SyntaxNode != null);

            OnCompile();
        }

        protected abstract void OnCompile();

        internal SelectNode SyntaxNode { get; private set; }

        public string Sql { get; private set; }

        internal Filter<TResult> ResultFilters { get; private set; }

        public override string ToString()
        {
            return Sql;
        }

        protected static IEqualityComparer<TResult> CreateDistinctComparer()
        {
            if (typeof(TResult).IsDictionary())
            {
                Type t = typeof(DictionaryComparer<,>).MakeGenericType(typeof(TResult).GetGenericArguments());

                return (IEqualityComparer<TResult>)Activator.CreateInstance(t);
            }
            else if (typeof(TResult) == typeof(object))
            {
                return (IEqualityComparer<TResult>)new ExpandoComparer();
            }

            return EqualityComparer<TResult>.Default;
        }
    }
}
