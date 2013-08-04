using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SqlLinq.SyntaxTree.Aggregates
{
    public abstract class CountNode : AggregateNode
    {
        protected CountNode()
        {
        }

        protected override Type GetEvaluatatorType()
        {
            return this.GetType();
        }

        protected override MethodInfo GetEvaluationMethod(Type paramType)
        {
            return (from method in GetEvaluatatorType().GetMethods(BindingFlags.Static | BindingFlags.Public)
                    where method.Name == Name
                    && method.GetParameters().Length == 1
                    select method).First();
        }

        protected override MethodCallExpression GetPropertyOrFieldAggregateExpression(Type tSource, Expression param)
        {
            string sourceFieldName = GetSourceFieldName();
            LambdaExpression lambda = ExpressionFactory.CreateFieldSelectorLambda(tSource, sourceFieldName);

            return Expression.Call(GetEvaluatatorType(), Name, new Type[] { tSource, tSource.GetFieldType(sourceFieldName) }, param, lambda);
        }
    }
}
