using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Diagnostics;

using SqlLinq.SyntaxTree.Expressions;
using SqlLinq.SyntaxTree.Aggregates;

namespace SqlLinq.SyntaxTree.Clauses
{
    [SyntaxNode(RuleConstants.RULE_GROUPCLAUSE_GROUP_BY)]
    public class GroupByClause : NonTerminalNode
    {
        public GroupByClause()
        {
        }

        internal override void CheckSyntax()
        {
            if (GroupByItems.Count() > 8)
                throw new SqlException("Only 8 fields or less are currently supported for grouping");

            base.CheckSyntax();
        }

        public IEnumerable<NodeWithId> GroupByItems
        {
            get
            {
                return FindDescendants<NodeWithId>();
            }
        }

        public Func<IEnumerable<TSource>, IEnumerable<TResult>> CreateGroupBySelector<TSource, TResult>(IEnumerable<AggregateNode> aggregates)
        {
            // figure out what types the multipart key consists of and get the correct geenric instance
            // we are using Tuples to hold the key
            IEnumerable<Type> types = GroupByItems.Select(g => typeof(TSource).GetFieldType(g.Id.LookupId));
            IEnumerable<string> sourceFields = GroupByItems.Select(g => g.Id.LookupId);
            Type tupleTemplate = GetTupleTemplate(types.Count());
            Type tupleType = tupleTemplate.MakeGenericType(types.ToArray());

            // this instance of the tuple will be the multi-part key the same way as if you did
            // from p in source
            // group p by new Tuple<string, string>(p.Name, p.Address) into g
            //
            // for the degenerate case of only a single part key like a string a Tuple<string> is used
            var newTuple = ExpressionFactory.CreateSelectIntoObjectConstructor<TSource>(tupleType, sourceFields, types);

            // the grouped subset passed to resultSelector
            var source = Expression.Parameter(typeof(IEnumerable<TSource>), "source");

            var aggregateDelegates = from a in aggregates
                                     select new KeyValuePair<string, Delegate>
                                     (
                                         a.ColumnAlias,
                                         Expression.Lambda(a.GetCallExpression(typeof(TSource), source), source).Compile()
                                     );

            // create an object to cache the aggregate expressions for the result selector
            // this is so we can invoke the aggregate on the grouped subset of data
            GroupByCall<TSource, TResult> groupingCall = GroupByCallFactory.Create<TSource, TResult>(sourceFields, aggregateDelegates);
            
            // create the call to the result selector
            var key = Expression.Parameter(tupleType, "key");

            var evaluate = Expression.Call(Expression.Constant(groupingCall), "Evaluate", null, key, source);
            var resultSelectorLambda = Expression.Lambda(evaluate, key, source);

            // package all of that up in a call to Enumerable.GroupBy
            var groupByExpression = Expression.Call(typeof(Enumerable), "GroupBy", new Type[] { typeof(TSource), tupleType, typeof(TResult) }, source, newTuple, resultSelectorLambda);

            // create the lamda and compile
            return Expression.Lambda<Func<IEnumerable<TSource>, IEnumerable<TResult>>>(groupByExpression, source).Compile();
        }

        private static Type GetTupleTemplate(int typeArgumentCount)
        {
            Debug.Assert(typeArgumentCount >= 1 && typeArgumentCount <= 8);
            switch (typeArgumentCount)
            {
                case 1: return typeof(Tuple<>);
                case 2: return typeof(Tuple<,>);
                case 3: return typeof(Tuple<,,>);
                case 4: return typeof(Tuple<,,,>);
                case 5: return typeof(Tuple<,,,,>);
                case 6: return typeof(Tuple<,,,,,>);
                case 7: return typeof(Tuple<,,,,,,>);
                case 8: return typeof(Tuple<,,,,,,,>);
            }

            throw new ArgumentException("Expressions greater than 8 fields are not supported.", "typeArgumentCount");
        }
    }
}