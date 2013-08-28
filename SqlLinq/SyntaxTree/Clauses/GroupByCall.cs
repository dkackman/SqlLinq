using System;
using System.Linq;
using System.Dynamic;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace SqlLinq.SyntaxTree.Clauses
{
    static class GroupByCallFactory
    {
        public static GroupByCall<TSource, TResult> Create<TSource, TResult>(IEnumerable<string> groupByFieldNames, IEnumerable<KeyValuePair<string, Delegate>> aggregateDelegates)
        {
            if (typeof(TResult) == typeof(object))
                return new GroupByExpando<TSource, TResult>(groupByFieldNames, aggregateDelegates);

            if (typeof(TResult).GetMethod("Add", typeof(TResult).GetGenericArguments()) != null)
                return new GroupByIntoDictionary<TSource, TResult>(groupByFieldNames, aggregateDelegates);

            if (typeof(TResult).HasEmptyConstructor())
                return new GroupByMemberInit<TSource, TResult>(groupByFieldNames, aggregateDelegates);

            return new GroupByConstructor<TSource, TResult>(groupByFieldNames, aggregateDelegates);
        }
    }

    abstract class GroupByCall<TSource, TResult>
    {
        protected GroupByCall(IEnumerable<string> groupByFieldNames, IEnumerable<KeyValuePair<string, Delegate>> aggregateDelegates)
        {
            Debug.Assert(groupByFieldNames != null);
            Debug.Assert(groupByFieldNames.Count() > 0);
            
            GroupByFieldNames = groupByFieldNames.ToList();
            Aggregates = aggregateDelegates;
        }

        protected IEnumerable<KeyValuePair<string, Delegate>> Aggregates { get; private set; }

        public IList<string> GroupByFieldNames { get; private set; }

        public abstract TResult Evaluate(object key, IEnumerable<TSource> source);

        /// <summary>
        /// Given an object which is a tuple returns a list of the values for each Item# property order by #
        /// </summary>
        /// <param name="tuple">An object that is a Tuple</param>
        /// <returns>A list of the values of each of the Tuple's Item properties, ordered by property name</returns>
        protected static IList<object> DereferenceTuple(object tuple)
        {
            var properties = from p in tuple.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)                             
                             where p.Name.StartsWith("Item")
                             orderby p.Name
                             select p;

            return properties.Select(p => p.GetValue(tuple)).ToList();
        }
    }

    class GroupByIntoDictionary<TSource, TResult> : GroupByCall<TSource, TResult>
    {
        public GroupByIntoDictionary(IEnumerable<string> groupByFieldNames, IEnumerable<KeyValuePair<string, Delegate>> aggregateDelegates)
            : base(groupByFieldNames, aggregateDelegates)
        {
        }

        public override TResult Evaluate(object key, IEnumerable<TSource> source)
        {
            Debug.Assert(key != null);
            Debug.Assert(key.GetType().IsTuple());

            Type dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(TResult).GetGenericArguments());

            object dictionary = Activator.CreateInstance(dictionaryType);
            MethodInfo addMethod = dictionaryType.GetMethod("Add", typeof(TResult).GetGenericArguments());

            // add the group by fields to the instance
            var groupByFieldValues = DereferenceTuple(key);
            for (int i = 0; i < GroupByFieldNames.Count; i++)
                addMethod.Invoke(dictionary, new object[] { GroupByFieldNames[i], groupByFieldValues[i] });

            // add the aggregate values to the instance
            foreach (KeyValuePair<string, Delegate> pair in Aggregates)
                addMethod.Invoke(dictionary, new object[] { pair.Key, pair.Value.DynamicInvoke(source) });

            return (TResult)dictionary;
        }
    }

    class GroupByExpando<TSource, TResult> : GroupByCall<TSource, TResult>
    {
        public GroupByExpando(IEnumerable<string> groupByFieldNames, IEnumerable<KeyValuePair<string, Delegate>> aggregateDelegates)
            : base(groupByFieldNames, aggregateDelegates)
        {
        }

        public override TResult Evaluate(object key, IEnumerable<TSource> source)
        {
            Debug.Assert(key != null);
            Debug.Assert(key.GetType().IsTuple());

            // add the group by fields to the instance
            IDictionary<string, object> expando = (IDictionary<string, object>)new ExpandoObject();
            var groupByFieldValues = DereferenceTuple(key);
            for (int i = 0; i < GroupByFieldNames.Count; i++)
                expando.Add(GroupByFieldNames[i], groupByFieldValues[i]);

            // add the aggregate values to the instance
            foreach (KeyValuePair<string, Delegate> pair in Aggregates)
                expando.Add(pair.Key, pair.Value.DynamicInvoke(source));

            return (TResult)(object)expando;
        }
    }

    class GroupByConstructor<TSource, TResult> : GroupByCall<TSource, TResult>
    {
        public GroupByConstructor(IEnumerable<string> groupByFieldNames, IEnumerable<KeyValuePair<string, Delegate>> aggregateDelegates)
            : base(groupByFieldNames, aggregateDelegates)
        {
        }

        public override TResult Evaluate(object key, IEnumerable<TSource> source)
        {
            Debug.Assert(key != null);
            Debug.Assert(key.GetType().IsTuple());

            // add the group by fields to the instance
            List<object> list = new List<object>(DereferenceTuple(key));

            // add the aggregate values to the instance
            foreach (KeyValuePair<string, Delegate> pair in Aggregates)
                list.Add(pair.Value.DynamicInvoke(source));

            return (TResult)Activator.CreateInstance(typeof(TResult), list.ToArray());
        }
    }

    class GroupByMemberInit<TSource, TResult> : GroupByCall<TSource, TResult>
    {
        public GroupByMemberInit(IEnumerable<string> groupByFieldNames, IEnumerable<KeyValuePair<string, Delegate>> aggregateDelegates)
            : base(groupByFieldNames, aggregateDelegates)
        {
        }

        public override TResult Evaluate(object key, IEnumerable<TSource> source)
        {
            Debug.Assert(key != null);
            Debug.Assert(key.GetType().IsTuple());

            TResult result = Activator.CreateInstance<TResult>();

            // add the group by fields to the instance
            var groupByFieldValues = DereferenceTuple(key);
            for (int i = 0; i < GroupByFieldNames.Count; i++)
                typeof(TResult).GetProperty(GroupByFieldNames[i]).SetValue(result, groupByFieldValues[i], null);

            // add the aggregate values to the instance
            foreach (KeyValuePair<string, Delegate> pair in Aggregates)
            {
                object o = pair.Value.DynamicInvoke(source);
                typeof(TResult).GetProperty(pair.Key).SetValue(result, o, null);
            }

            return result;
        }
    }
}
