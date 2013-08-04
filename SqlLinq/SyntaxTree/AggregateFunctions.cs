using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace SqlLinq.SyntaxTree
{
    static class AggregateFunctionsFactory
    {
        public static AggregateFunctions<TSource, TResult> Create<TSource, TResult>(IDictionary<string, Delegate> delegates)
        {
            if (typeof(TResult) == typeof(object))
                return new AggregateIntoExpando<TSource, TResult>(delegates);

            else if (typeof(TResult).IsDictionary())
                return new AggregateIntoDictionary<TSource, TResult>(delegates);

            else if (typeof(TResult).HasEmptyConstructor())
                return new AggregateIntoObjectMembers<TSource, TResult>(delegates);

            return new AggregateIntoObjectConstructor<TSource, TResult>(delegates);
        }
    }

    abstract class AggregateFunctions<TSource, TResult>
    {
        protected AggregateFunctions(IDictionary<string, Delegate> delegates)
        {
            Aggregates = delegates;
        }

        protected IDictionary<string, Delegate> Aggregates { get; private set; }

        public IEnumerable<TResult> Evaluate(IEnumerable<TSource> source)
        {
            Debug.Assert(source != null);
            return new List<TResult>
            {
                GetResult(source)
            };
        }

        protected abstract TResult GetResult(IEnumerable<TSource> source);
    }

    class AggregateIntoExpando<TSource, TResult> : AggregateFunctions<TSource, TResult>
    {
        public AggregateIntoExpando(IDictionary<string, Delegate> delegates)
            : base(delegates)
        {
        }

        protected override TResult GetResult(IEnumerable<TSource> source)
        {
            IDictionary<string, object> expando = (IDictionary<string, object>)new ExpandoObject();

            foreach (KeyValuePair<string, Delegate> pair in Aggregates)
                expando.Add(pair.Key, pair.Value.DynamicInvoke(source));

            return (TResult)expando;
        }
    }

    class AggregateIntoDictionary<TSource, TResult> : AggregateFunctions<TSource, TResult>
    {
        public AggregateIntoDictionary(IDictionary<string, Delegate> delegates)
            : base(delegates)
        {
        }

        protected override TResult GetResult(IEnumerable<TSource> source)
        {
            Type dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(TResult).GetGenericArguments());

            object dictionary = Activator.CreateInstance(dictionaryType);
            MethodInfo addMethod = dictionaryType.GetMethod("Add", typeof(TResult).GetGenericArguments());

            foreach (KeyValuePair<string, Delegate> pair in Aggregates)
                addMethod.Invoke(dictionary, new object[] { pair.Key, pair.Value.DynamicInvoke(source) });

            return (TResult)dictionary;
        }
    }

    class AggregateIntoObjectMembers<TSource, TResult> : AggregateFunctions<TSource, TResult>
    {
        public AggregateIntoObjectMembers(IDictionary<string, Delegate> delegates)
            : base(delegates)
        {
        }

        protected override TResult GetResult(IEnumerable<TSource> source)
        {
            TResult result = Activator.CreateInstance<TResult>();

            foreach (KeyValuePair<string, Delegate> pair in Aggregates)
            {
                object o = pair.Value.DynamicInvoke(source);
                typeof(TResult).GetProperty(pair.Key).SetValue(result, o, null);
            }

            return result;
        }
    }

    class AggregateIntoObjectConstructor<TSource, TResult> : AggregateFunctions<TSource, TResult>
    {
        public AggregateIntoObjectConstructor(IDictionary<string, Delegate> delegates)
            : base(delegates)
        {
        }

        protected override TResult GetResult(IEnumerable<TSource> source)
        {
            List<object> list = new List<object>();

            foreach (KeyValuePair<string, Delegate> pair in Aggregates)
                list.Add(pair.Value.DynamicInvoke(source));

            // special case for value type scalar queries
            if (typeof(TResult).IsValueType && list.Count == 1)
                return (TResult)list[0];

            // tuples are initialized here
            return (TResult)Activator.CreateInstance(typeof(TResult), list.ToArray());
        }
    }
}
