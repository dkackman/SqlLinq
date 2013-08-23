using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Diagnostics;

using SqlLinq.SyntaxTree.Aggregates;

namespace SqlLinq.SyntaxTree
{
    [SyntaxNode(RuleConstants.RULE_COLUMNS)]
    public class Columns : NonTerminalNode
    {
        public Columns()
        {
        }

        public bool Distinct
        {
            get
            {
                return FindChild<NonTerminalNode>(RuleConstants.RULE_RESTRICTION_DISTINCT) != null;
            }
        }

        public bool IsAggregateOnlyQuery
        {
            get
            {   // we have only calculated fields
                return Aggregates.Any() && Aggregates.Count() == ColumnSources.Count();
            }
        }

        public IEnumerable<AggregateNode> Aggregates
        {
            get
            {
                return FindDescendants<AggregateNode>();
            }
        }

        public Func<IEnumerable<TSource>, TResult> CreateScalarSelector<TSource, TResult>(Func<IEnumerable<TSource>, IEnumerable<object>> selector)
        {
            ParameterExpression source = Expression.Parameter(typeof(IEnumerable<TSource>), "source");
            var eval = Expression.Call(Expression.Constant(selector.Target), selector.Method, source);
            var _get = Expression.Call(GetType(), "GetScalarValue", new Type[] { typeof(TResult) }, eval);

            return Expression.Lambda<Func<IEnumerable<TSource>, TResult>>(_get, source).Compile();
        }

        public Func<IEnumerable<TSource>, IEnumerable<TInner>, TResult> CreateScalarSelector<TSource, TInner, TResult>(Func<IEnumerable<TSource>, IEnumerable<TInner>, IEnumerable<object>> selector)
        {
            ParameterExpression source = Expression.Parameter(typeof(IEnumerable<TSource>), "source");
            var eval = Expression.Call(Expression.Constant(selector.Target), selector.Method, source);
            var _get = Expression.Call(GetType(), "GetScalarValue", new Type[] { typeof(TResult) }, eval);

            return Expression.Lambda<Func<IEnumerable<TSource>, IEnumerable<TInner>, TResult>>(_get, source).Compile();
        }

        private static TResult GetScalarValue<TResult>(IEnumerable<object> resultList)
        {
            object ret = resultList.First();

            // need to think about the actual behavior of executescalar 
            // - this bit here allows subqueries to return objects
            // wrapped in expando objects - this current implementation makes execute scalar flaky around the edges
            if (ret is IDictionary<string, object>)
                return (TResult)((IDictionary<string, object>)ret).Values.First();

            if (ret.GetType().IsTuple())
                return (TResult)ret.GetPropertyOrFieldValue("Item1");

            return (TResult)ret;
        }

        public virtual Func<IEnumerable<TSource>, IEnumerable<TResult>> CreateSelector<TSource, TResult>()
        {
            if (IsAggregateOnlyQuery)
                return CreateAggregateSelector<TSource, TResult>();

            return list => list.Select(CreateReturnFunction<TSource, TResult>());
        }

        private Func<IEnumerable<TSource>, IEnumerable<TResult>> CreateAggregateSelector<TSource, TResult>()
        {
            var arg = Expression.Parameter(typeof(IEnumerable<TSource>), "arg");

            // for each aggregate in the query create a lambda expression and add it to the cache
            var delegates = (from a in Aggregates
                    select new 
                    {
                        Name = a.Alias,
                        Func = Expression.Lambda(a.GetCallExpression(typeof(TSource), arg), arg).Compile()                        
                    }).ToDictionary(o => o.Name, o => o.Func, StringComparer.OrdinalIgnoreCase);

            // create an object to cache some state for the result selector
            AggregateFunctions<TSource, TResult> aggregateSet = AggregateFunctionsFactory.Create<TSource, TResult>(delegates);

            var evaluate = Expression.Call(Expression.Constant(aggregateSet), "Evaluate", null, arg);

            return Expression.Lambda<Func<IEnumerable<TSource>, IEnumerable<TResult>>>(evaluate, arg).Compile();
        }

        private Func<TSource, TResult> CreateReturnFunction<TSource, TResult>()
        {
            IEnumerable<string> fields = GetFieldList();
            IEnumerable<string> results = GetResultList();
            Debug.Assert(fields.Any() && results.Any());

            // if TResult is a dictionary
            if (typeof(TResult).IsDictionary())
                return ExpressionFactory.CreateSelectIntoDictionary<TSource, TResult>(fields, results).Compile();

            //// if selecting only one field create a simple selection function
            if (results.Count() == 1)
                return ExpressionFactory.CreateSelectSingleField<TSource, TResult>(fields.First()).Compile();

            // if TResult is dynamic return an expando object
            if (typeof(TResult) == typeof(object))
                return ExpressionFactory.CreateSelectIntoExpando<TSource, TResult>(fields, results).Compile();

            // if return type has an empty constructor use it and memberinit
            if (typeof(TResult).HasEmptyConstructor())
                return ExpressionFactory.CreateSelectIntoObjectMembers<TSource, TResult>(fields, results).Compile();

            // otherwise try and use a constructor that matches the field list types - specifically useful for Tuple<>            
            IEnumerable<Type> resultTypes = GetResultTypes(typeof(TResult));
            return ExpressionFactory.CreateSelectIntoObjectConstructor<TSource, TResult>(fields, resultTypes).Compile();
        }

        public IEnumerable<string> GetFieldList()
        {
            return (from c in ColumnSources
                    from f in c.GetFields()
                    select f).Select(field => field.LookupId).Distinct(StringComparer.OrdinalIgnoreCase);
        }

        public IEnumerable<string> GetResultList()
        {
            return (from c in ColumnSources
                    select c.Alias).Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private IEnumerable<Type> GetResultTypes(Type tResult)
        {
            var columns = ColumnSources;
            return from c in columns
                   select tResult.GetFieldType(c.Id.LocalId, columns.IndexOf(c));
        }

        public IEnumerable<ColumnSource> ColumnSources
        {
            get
            {
                // depending on how the select statement is contructed the parser may or may not elimate some intermediate reductions
                // the where statement here will elimate those instances where reductions are not elimated by the parser        
                return FindDescendants<ColumnSource>().Where(item => item.FindChild<ColumnSource>() == null);
            }
        }
    }
}
