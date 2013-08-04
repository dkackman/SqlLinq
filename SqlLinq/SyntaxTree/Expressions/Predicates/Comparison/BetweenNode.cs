using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Expressions.Predicates.Comparison
{
    [SyntaxNode(RuleConstants.RULE_PREDEXP_BETWEEN_AND)]
    public class BetweenNode : ExpressionNode
    {
        public BetweenNode()
        {
        }

        internal override Expression CreateExpression(ParameterExpression sourceData, ParameterExpression param)
        {
            return CreateBetweenExpression(sourceData, param, CreateChildExpression(sourceData, param, 2), CreateChildExpression(sourceData, param, 4));
        }

        protected Expression CreateBetweenExpression(ParameterExpression sourceData, ParameterExpression param, Expression start, Expression end)
        {           
            var expression = CreateChildExpression(sourceData, param, 0);

            Type widest = expression.Type.Widest(start.Type, end.Type);
            Debug.Assert(widest != null);

            if (expression.Type != widest)
                expression = Expression.Convert(expression, widest);

            if (start.Type != widest)
                start = Expression.Convert(start, widest);

            if (end.Type != widest)
                end = Expression.Convert(end, widest);

            Expression gte = Expression.GreaterThanOrEqual(expression, start);
            Expression lte = Expression.LessThanOrEqual(expression, end);

            return Expression.TryCatch(Expression.And(gte, lte), Expression.Catch(typeof(Exception), Expression.Constant(false)));
        }
    }
}
