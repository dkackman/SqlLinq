using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace SqlLinq.SyntaxTree.Clauses
{
    static class GroupByCallFactory
    {
        public static GroupByCall<TSource, TResult> Create<TSource, TResult>(string keyName)
        {
            if (typeof(TResult) == typeof(object))
                return new GroupByExpando<TSource, TResult>(keyName);

            if (typeof(TResult).GetMethod("Add", typeof(TResult).GetGenericArguments()) != null)
                return new GroupByIntoDictionary<TSource, TResult>(keyName);

            if (typeof(TResult).HasEmptyConstructor())
                return new GroupByMemberInit<TSource, TResult>(keyName);

            return new GroupByConstructor<TSource, TResult>(keyName);
        }
    }

    abstract class GroupByCall<TSource, TResult>
    {
        protected GroupByCall(string keyName)
        {
            Debug.Assert(!string.IsNullOrEmpty(keyName));

            Aggregates = new Dictionary<string, Delegate>(StringComparer.OrdinalIgnoreCase);
            KeyName = keyName;
        }

        public IDictionary<string, Delegate> Aggregates { get; private set; }

        public string KeyName { get; private set; }

        public abstract TResult Evaluate(object key, IEnumerable<TSource> source);
    }

    class GroupByIntoDictionary<TSource, TResult> : GroupByCall<TSource, TResult>
    {
        public GroupByIntoDictionary(string keyName)
            : base(keyName)
        {
        }

        public override TResult Evaluate(object key, IEnumerable<TSource> source)
        {
            Type dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(TResult).GetGenericArguments());

            object dictionary = Activator.CreateInstance(dictionaryType);
            MethodInfo addMethod = dictionaryType.GetMethod("Add", typeof(TResult).GetGenericArguments());

            // KeyName is the field being grouped on - so add it
            addMethod.Invoke(dictionary, new object[] { KeyName, key });

            foreach (KeyValuePair<string, Delegate> pair in Aggregates)
                addMethod.Invoke(dictionary, new object[] { pair.Key, pair.Value.DynamicInvoke(source) });

            return (TResult)dictionary;
        }
    }

    class GroupByExpando<TSource, TResult> : GroupByCall<TSource, TResult>
    {
        public GroupByExpando(string keyName)
            : base(keyName)
        {
        }

        public override TResult Evaluate(object key, IEnumerable<TSource> source)
        {
            IDictionary<string, object> expando = (IDictionary<string, object>)new ExpandoObject();
            expando.Add(KeyName, key);

            foreach (KeyValuePair<string, Delegate> pair in Aggregates)
                expando.Add(pair.Key, pair.Value.DynamicInvoke(source));

            return (TResult)(object)expando;
        }
    }

    class GroupByConstructor<TSource, TResult> : GroupByCall<TSource, TResult>
    {
        public GroupByConstructor(string keyName)
            : base(keyName)
        {
        }

        public override TResult Evaluate(object key, IEnumerable<TSource> source)
        {
            List<object> list = new List<object>();
            list.Add(key);

            foreach (KeyValuePair<string, Delegate> pair in Aggregates)
                list.Add(pair.Value.DynamicInvoke(source));

            return (TResult)Activator.CreateInstance(typeof(TResult), list.ToArray());
        }
    }

    class GroupByMemberInit<TSource, TResult> : GroupByCall<TSource, TResult>
    {
        public GroupByMemberInit(string keyName)
            : base(keyName)
        {
        }

        public override TResult Evaluate(object key, IEnumerable<TSource> source)
        {
            TResult result = Activator.CreateInstance<TResult>();
            typeof(TResult).GetProperty(KeyName).SetValue(result, key, null);

            foreach (KeyValuePair<string, Delegate> pair in Aggregates)
            {
                object o = pair.Value.DynamicInvoke(source);
                typeof(TResult).GetProperty(pair.Key).SetValue(result, o, null);
            }

            return result;
        }
    }
}
