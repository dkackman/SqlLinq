using System;
using System.Linq.Expressions;
using System.Diagnostics;

namespace SqlLinq.SyntaxTree.Expressions
{
    public abstract class BinaryExpressionNode : ExpressionNode
    {
        protected BinaryExpressionNode()
        {
        }

        internal override Expression CreateExpression(ParameterExpression sourceData, ParameterExpression param)
        {
            var left = CreateChildExpression(sourceData, param, 0);
            var right = CreateChildExpression(sourceData, param, 2);

            Type widest = right.Type.Widest(left.Type);
            Debug.Assert(widest != null);

            if (right.Type != widest)
                right = Expression.Convert(right, widest);

            if (left.Type != widest)
                left = Expression.Convert(left, widest);

            var body = CreateChildExpression(left, right);
            return WrapReturnExpression(body);
        }

        protected virtual Expression WrapReturnExpression(Expression body)
        {
            return body;
        }

        protected abstract Expression CreateChildExpression(Expression left, Expression right);
    }
}
