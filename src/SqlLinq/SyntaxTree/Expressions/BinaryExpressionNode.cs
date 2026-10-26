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
            return CreateBinaryExpression(CreateChildExpression(sourceData, param, 0), CreateChildExpression(sourceData, param, 2));
        }

        protected Expression CreateBinaryExpression(Expression left, Expression right)
        {
            Type widest = right.Type.Widest(left.Type);
            Debug.Assert(widest != null);

            if (right.Type != widest)
            {
                right = ExpressionFactory.ConvertTo(right, widest);
            }

            if (left.Type != widest)
            {
                left = ExpressionFactory.ConvertTo(left, widest);
            }

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
