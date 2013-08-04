using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;

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

        public IEnumerable<NodeWithId> GroupByItems
        {
            get
            {
                return FindDescendants<NodeWithId>();
            }
        }

        public Func<IEnumerable<TSource>, IEnumerable<TResult>> CreateGroupBySelector<TSource, TResult>(IEnumerable<AggregateNode> aggregates)
        {
            NodeWithId groupByField = GroupByItems.First();  // only one grouping field is supported atm
            Type keyType = typeof(TSource).GetFieldType(groupByField.Id.LookupId);       

            // create the key selector
            var keyLambda = ExpressionFactory.CreateFieldSelectorLambda(typeof(TSource), groupByField.Id.LookupId);

            // the grouped subset passed to resultSelector
            var source = Expression.Parameter(typeof(IEnumerable<TSource>), "source");

            // create an object to cache some state for the result selector
            GroupByCall<TSource, TResult> groupingCall = GroupByCallFactory.Create<TSource, TResult>(groupByField.Id.LookupId);

            // for each aggregate in the query create a lambda expression and add it to the cache
            foreach (AggregateNode aggregate in aggregates)
            {
                var aggregateExpression = aggregate.GetCallExpression(typeof(TSource), source);
                groupingCall.Aggregates.Add(aggregate.Alias, Expression.Lambda(aggregateExpression, source).Compile());
            }

            // create the call to the result selector
            var key = Expression.Parameter(keyType, "key");

            var evaluate = Expression.Call(Expression.Constant(groupingCall), "Evaluate", null, key, source);
            var resultSelectorLambda = Expression.Lambda(evaluate, key, source);

            // package all of that up in a call to Enumerable.GroupBy
            var groupByExpression = Expression.Call(typeof(Enumerable), "GroupBy", new Type[] { typeof(TSource), keyType, typeof(TResult) }, source, keyLambda, resultSelectorLambda);

            // create the lamda and compile
            return Expression.Lambda<Func<IEnumerable<TSource>, IEnumerable<TResult>>>(groupByExpression, source).Compile();
        }
    }
}
