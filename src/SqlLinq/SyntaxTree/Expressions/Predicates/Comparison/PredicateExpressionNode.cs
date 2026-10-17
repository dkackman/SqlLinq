using System;
using System.Reflection;
using System.Linq.Expressions;
using System.Diagnostics;

namespace SqlLinq.SyntaxTree.Expressions.Predicates.Comparison
{
    public abstract class PredicateExpressionNode : BinaryExpressionNode
    {
        protected override Expression WrapReturnExpression(Expression body)
        {
            return Expression.TryCatch(body, Expression.Catch(typeof(Exception), Expression.Constant(false)));
        }
    }
}
