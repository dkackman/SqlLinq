using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Joins
{
    /// <summary>
    /// These guys abstract away knowing how to get the values that will be
    /// joined on as expressions are built
    /// </summary>
    interface IJoinEntry
    {
        bool ContainsKey(string key);

        object this[string key] { get; }
    }

    class ObjectJoinEntry : IJoinEntry
    {
        private object _entry;

        public ObjectJoinEntry(object o)
        {
            Debug.Assert(o != null);

            _entry = o;
        }

        public bool ContainsKey(string key)
        {
            return _entry.GetType().HasPropertyOrField(key);
        }

        public object this[string key]
        {
            get
            {
                Debug.Assert(_entry.GetType().HasPropertyOrField(key));

                return _entry.GetPropertyOrFieldValue(key);
            }
        }
    }

    class DictionaryJoinEntry : IJoinEntry
    {
        private Func<string, object> _get_Item;
        private Func<string, bool> _containsKey;

        public DictionaryJoinEntry(object o, Type t)
        {
            Debug.Assert(o != null);

            _containsKey = CreateFunc<string, bool>(o, t, "ContainsKey");
            _get_Item = CreateFunc<string, object>(o, t, "get_Item");
        }

        public bool ContainsKey(string key)
        {
            return _containsKey(key);
        }

        public object this[string key]
        {
            get
            {
                return _get_Item(key);
            }
        }

        private static Func<TArg, TReturn> CreateFunc<TArg, TReturn>(object instance, Type t, string methodName)
        {
            var method = t.GetMethod(methodName, new Type[] { typeof(string) });
            Debug.Assert(method != null);

            ParameterExpression arg = Expression.Parameter(typeof(TArg), "arg");
            Expression call = Expression.Call(Expression.Constant(instance), method, arg);

            return Expression.Lambda<Func<TArg, TReturn>>(call, arg).Compile();
        }
    }
}
