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
        public static GroupByCall<TSource, TResult> Create<TSource, TResult>(IList<string> keyNames)
        {
            if (typeof(TResult) == typeof(object))
                return new GroupByExpando<TSource, TResult>(keyNames);

            if (typeof(TResult).GetMethod("Add", typeof(TResult).GetGenericArguments()) != null)
                return new GroupByIntoDictionary<TSource, TResult>(keyNames);

            if (typeof(TResult).HasEmptyConstructor())
                return new GroupByMemberInit<TSource, TResult>(keyNames);

            return new GroupByConstructor<TSource, TResult>(keyNames);
        }
    }

    abstract class GroupByCall<TSource, TResult>
    {
        protected GroupByCall(IList<string> keyNames)
        {
            Debug.Assert(keyNames != null);
            Debug.Assert(keyNames.Count > 0);
            
            Aggregates = new Dictionary<string, Delegate>(StringComparer.OrdinalIgnoreCase);
            KeyNames = keyNames;
        }

        public IDictionary<string, Delegate> Aggregates { get; private set; }

        public IList<string> KeyNames { get; private set; }

        public abstract TResult Evaluate(object key, IEnumerable<TSource> source);

        /// <summary>
        /// Given an object which is a tuple returns a list of the values for each Item# property order by #
        /// </summary>
        /// <param name="tuple"></param>
        /// <returns></returns>
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
        public GroupByIntoDictionary(IList<string> keyNames)
            : base(keyNames)
        {
        }

        public override TResult Evaluate(object key, IEnumerable<TSource> source)
        {
            Debug.Assert(key != null);
            Debug.Assert(key.GetType().IsTuple());

            Type dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(TResult).GetGenericArguments());

            object dictionary = Activator.CreateInstance(dictionaryType);
            MethodInfo addMethod = dictionaryType.GetMethod("Add", typeof(TResult).GetGenericArguments());

            var keyValues = DereferenceTuple(key);
            for (int i = 0; i < KeyNames.Count; i++)
                addMethod.Invoke(dictionary, new object[] { KeyNames[i], keyValues[i] });

            foreach (KeyValuePair<string, Delegate> pair in Aggregates)
                addMethod.Invoke(dictionary, new object[] { pair.Key, pair.Value.DynamicInvoke(source) });

            return (TResult)dictionary;
        }
    }

    class GroupByExpando<TSource, TResult> : GroupByCall<TSource, TResult>
    {
        public GroupByExpando(IList<string> keyNames)
            : base(keyNames)
        {
        }

        public override TResult Evaluate(object key, IEnumerable<TSource> source)
        {
            Debug.Assert(key != null);
            Debug.Assert(key.GetType().IsTuple());

            IDictionary<string, object> expando = (IDictionary<string, object>)new ExpandoObject();
            var keyValues = DereferenceTuple(key);
            for (int i = 0; i < KeyNames.Count; i++)
                expando.Add(KeyNames[i], keyValues[i]);

            foreach (KeyValuePair<string, Delegate> pair in Aggregates)
                expando.Add(pair.Key, pair.Value.DynamicInvoke(source));

            return (TResult)(object)expando;
        }
    }

    class GroupByConstructor<TSource, TResult> : GroupByCall<TSource, TResult>
    {
        public GroupByConstructor(IList<string> keyNames)
            : base(keyNames)
        {
        }

        public override TResult Evaluate(object key, IEnumerable<TSource> source)
        {
            Debug.Assert(key != null);
            Debug.Assert(key.GetType().IsTuple());

            List<object> list = new List<object>(DereferenceTuple(key));

            foreach (KeyValuePair<string, Delegate> pair in Aggregates)
                list.Add(pair.Value.DynamicInvoke(source));

            return (TResult)Activator.CreateInstance(typeof(TResult), list.ToArray());
        }
    }

    class GroupByMemberInit<TSource, TResult> : GroupByCall<TSource, TResult>
    {
        public GroupByMemberInit(IList<string> keyNames)
            : base(keyNames)
        {
        }

        public override TResult Evaluate(object key, IEnumerable<TSource> source)
        {
            Debug.Assert(key != null);
            Debug.Assert(key.GetType().IsTuple());

            TResult result = Activator.CreateInstance<TResult>();

            var keyValues = DereferenceTuple(key);
            for (int i = 0; i < KeyNames.Count; i++)
                typeof(TResult).GetProperty(KeyNames[i]).SetValue(result, keyValues[i], null);

            foreach (KeyValuePair<string, Delegate> pair in Aggregates)
            {
                object o = pair.Value.DynamicInvoke(source);
                typeof(TResult).GetProperty(pair.Key).SetValue(result, o, null);
            }

            return result;
        }
    }
}
