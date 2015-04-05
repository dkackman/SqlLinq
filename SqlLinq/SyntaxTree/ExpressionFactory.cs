using System;
using System.Linq;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace SqlLinq
{
    static class ExpressionFactory
    {
        public static Expression<Func<TSource, TResult>> CreateIdentitySelector<TSource, TResult>()
        {
            ParameterExpression item = Expression.Parameter(typeof(TSource), "item");

            return Expression.Lambda<Func<TSource, TResult>>(item, item);
        }

        public static Expression<Func<TSource, TResult>> CreateSelectIntoDictionary<TSource, TResult>(IEnumerable<string> sourceFields, IEnumerable<string> resultFields)
        {
            ParameterExpression item = Expression.Parameter(typeof(TSource), "item");
            var bindings = CreateDictionaryBindings<TResult>(item, sourceFields, resultFields);

            ConstructorInfo constructor = typeof(TResult).GetConstructor(new Type[] { typeof(StringComparer) });
            if (constructor == null)
            {
                Debug.Assert(typeof(TResult).IsGenericType);
                Debug.Assert(typeof(TResult).GetGenericArguments().Length == 2);
                Debug.Assert(typeof(TResult).GetGenericArguments()[0] == typeof(string));

                Type def = typeof(Dictionary<,>).MakeGenericType(typeof(TResult).GetGenericArguments());
                constructor = def.GetConstructor(new Type[] { typeof(StringComparer) });
            }
            Debug.Assert(constructor != null);

            var _new = Expression.New(constructor, Expression.Constant(StringComparer.OrdinalIgnoreCase));
            return Expression.Lambda<Func<TSource, TResult>>(Expression.ListInit(_new, bindings), item);
        }

        public static Expression<Func<TSource, TResult>> CreateSelectIntoExpando<TSource, TResult>(IEnumerable<string> sourceFields, IEnumerable<string> resultFields)
        {
            ParameterExpression item = Expression.Parameter(typeof(TSource), "item");
            var bindings = CreateDictionaryBindings<IDictionary<string, object>>(item, sourceFields, resultFields);

            var _new = Expression.New(typeof(ExpandoObject));
            return Expression.Lambda<Func<TSource, TResult>>(Expression.ListInit(_new, bindings), item);
        }

        private static IEnumerable<ElementInit> CreateDictionaryBindings<TResult>(ParameterExpression item, IEnumerable<string> sourceFields, IEnumerable<string> resultFields)
        {
            // loop through all of the result fields and generate an expression that will 
            // add a new property to the result expando object using its IDictionary interface
            MethodInfo addMethod = typeof(TResult).GetMethod("Add", typeof(TResult).GetGenericArguments());

            return from field in
                       sourceFields.Zip(resultFields, (source, result) => new Tuple<string, string>(source, result))
                   select
                       Expression.ElementInit(addMethod,
                       Expression.Constant(field.Item2),
                       CreateFieldSelector(item, field.Item1, typeof(TResult).GetDictionaryValueType()));
        }

        public static Expression<Func<TSource, TResult>> CreateSelectIntoObjectMembers<TSource, TResult>(IEnumerable<string> sourceFields, IEnumerable<string> resultFields)
        {
            ParameterExpression item = Expression.Parameter(typeof(TSource), "item");

            var bindings = from field in
                               sourceFields.Zip(resultFields, (source, result) => new Tuple<string, string>(source, result))
                           select
                               Expression.Bind(typeof(TResult).GetPropertyOrField(field.Item2), CreateFieldSelector(item, field.Item1, typeof(TResult).GetFieldType(field.Item2)));

            return Expression.Lambda<Func<TSource, TResult>>(Expression.MemberInit(Expression.New(typeof(TResult)), bindings), item);
        }

        public static Expression<Func<TSource, TResult>> CreateSelectIntoObjectConstructor<TSource, TResult>(IEnumerable<string> sourceFields, IEnumerable<Type> resultTypes)
        {
            ParameterExpression item = Expression.Parameter(typeof(TSource), "item");
            Expression _new = CreateNewObjectExpression(typeof(TResult), sourceFields, resultTypes, item);

            return Expression.Lambda<Func<TSource, TResult>>(_new, item);
        }

        public static LambdaExpression CreateSelectIntoObjectConstructor<TSource>(Type tResult, IEnumerable<string> sourceFields, IEnumerable<Type> resultTypes)
        {
            ParameterExpression item = Expression.Parameter(typeof(TSource), "item");
            Expression _new = CreateNewObjectExpression(tResult, sourceFields, resultTypes, item);

            return Expression.Lambda(_new, item);
        }

        private static Expression CreateNewObjectExpression(Type tResult, IEnumerable<string> sourceFields, IEnumerable<Type> resultTypes, ParameterExpression item)
        {
            var zip = sourceFields.Zip(resultTypes, (s, t) => new Tuple<string, Type>(s, t));

            var bindings = zip.Select(tuple => CreateFieldSelector(item, tuple.Item1, tuple.Item2));   // the values that will intialize a TResult

            ConstructorInfo constructor = tResult.GetConstructor(resultTypes.ToArray());  // the constructor for a new TResult
            Debug.Assert(constructor != null);

            return Expression.New(constructor, bindings);
        }

        public static Expression<Func<TSource, TResult>> CreateSelectSingleField<TSource, TResult>(string propertyName)
        {
            Debug.Assert(!string.IsNullOrEmpty(propertyName));

            ParameterExpression item = Expression.Parameter(typeof(TSource), "item");

            return Expression.Lambda<Func<TSource, TResult>>(CreateFieldSelector(item, propertyName, typeof(TResult)), item);
        }

        public static Expression CreateFieldSelector(ParameterExpression item, string propertyName)
        {
            // source type is dynamic
            if (item.Type == typeof(object))
            {
                return Expression.Dynamic(new DynamicGetMemberBinder(propertyName), item.Type, item);
            }

            // source type is dictionary
            MethodInfo indexerMethod = item.Type.GetMethod("get_Item", new Type[] { typeof(string) });
            if (indexerMethod != null)
            {
                return Expression.Call(item, indexerMethod, Expression.Constant(propertyName));
            }            

            // source type is a class or stuct
            return Expression.PropertyOrField(item, propertyName);
        }

        public static Expression CreateFieldSelector(ParameterExpression item, string propertyName, Type returnType)
        {
            Expression selector = CreateFieldSelector(item, propertyName);

            if (selector.Type != returnType)
                selector = Expression.Convert(selector, returnType);

            return selector;
        }

        public static LambdaExpression CreateFieldSelectorLambda(Type itemType, string propertyName)
        {
            Debug.Assert(!string.IsNullOrEmpty(propertyName));

            ParameterExpression item = Expression.Parameter(itemType, "item");

            return Expression.Lambda(CreateFieldSelector(item, propertyName), item);
        }

        public static Expression CreateFieldSelectorLambda(ParameterExpression item, string propertyName, Type returnType)
        {
            Debug.Assert(!string.IsNullOrEmpty(propertyName));

            return Expression.Lambda(CreateFieldSelector(item, propertyName, returnType), item);
        }
    }
}
