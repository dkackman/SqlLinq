using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Reflection;

namespace SqlLinq.SyntaxTree.Expressions
{
    public abstract class ExpressionNode : NonTerminalNode
    {
        protected ExpressionNode()
        {
        }

        public Func<IEnumerable<T>, T, bool> CreatePredicateFunction<T>(ParameterExpression sourceData)
        {
            var param = Expression.Parameter(typeof(T), "arg");

            //traverse the tree to generate a lambda expression and then compile into a function
            var expression = CreateExpression(sourceData, param);

            // if the expression doesn't return a boolean add a clause to convert the result to bool
            if (expression.Type != typeof(bool))
            {
                MethodInfo convert = typeof(Convert).GetMethod("ToBoolean", new Type[] { expression.Type });
                Debug.Assert(convert != null);

                expression = Expression.Convert(expression, typeof(bool), convert);
            }

            return Expression.Lambda<Func<IEnumerable<T>, T, bool>>(expression, sourceData, param).Compile();
        }

        internal abstract Expression CreateExpression(ParameterExpression sourceData, ParameterExpression param);

        protected Expression CreateChildExpression(ParameterExpression sourceData, ParameterExpression param, int index)
        {
            ExpressionNode expression = FindChild<ExpressionNode>(index);
            Debug.Assert(expression != null);
            return expression.CreateExpression(sourceData, param);
        }
    }
}
