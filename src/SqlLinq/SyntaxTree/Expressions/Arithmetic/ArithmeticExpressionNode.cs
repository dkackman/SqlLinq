using System;
using System.Globalization;
using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Expressions.Arithmetic
{
    public abstract class ArithmeticExpressionNode : BinaryExpressionNode
    {
        protected ArithmeticExpressionNode()
        {
        }

        internal override Expression CreateExpression(ParameterExpression sourceData, ParameterExpression param)
        {
            var left = CreateChildExpression(sourceData, param, 0);
            var right = CreateChildExpression(sourceData, param, 2);

            if (left.Type != typeof(object) && right.Type != typeof(object))
            {
                return CreateBinaryExpression(left, right);
            }

            return Expression.Convert(CreateChildExpression(ToNumber(left), ToNumber(right)), typeof(object));
        }

        internal static Expression ToNumber(Expression value)
        {
            return Expression.Call(new Func<object, double?>(ToNumber).Method, Expression.Convert(value, typeof(object)));
        }

        private static double? ToNumber(object value)
        {
            if (value == null)
            {
                return null;
            }

            var code = Type.GetTypeCode(value.GetType());
            if (value is Enum || code < TypeCode.SByte || code > TypeCode.Decimal)
            {
                throw new SqlException("Arithmetic requires numeric operands; received " + value.GetType().Name + ".");
            }

            return Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }
    }
}
