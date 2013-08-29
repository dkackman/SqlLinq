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

        protected override Type EvaluatatorType
        {
            get
            {
                return this.GetType();
            }
        }

        protected override MethodInfo GetEvaluationMethod(Type paramType)
        {
            return (from method in EvaluatatorType.GetMethods(BindingFlags.Static | BindingFlags.Public)
                    where method.Name == Name
                    && method.GetParameters().Length == 1
                    select method).First();
        }

        protected override MethodCallExpression GetPropertyOrFieldAggregateExpression(Type tSource, Expression param)
        {
            LambdaExpression lambda = ExpressionFactory.CreateFieldSelectorLambda(tSource, SourceFieldName);

            return Expression.Call(EvaluatatorType, Name, new Type[] { tSource, tSource.GetFieldType(SourceFieldName) }, param, lambda);
        }
    }
}
