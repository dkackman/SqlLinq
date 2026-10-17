using System;
using System.Linq;
using System.Diagnostics;
using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Expressions.Predicates.Comparison
{
    [SyntaxNode(RuleConstants.RULE_PREDEXP_IN)]
    public class InNode : ExpressionNode
    {
        public InNode()
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        protected Expression CreateInExpression(Expression list, Expression left)
        {
            Debug.Assert(list.Type.IsArray);

            var listType = list.Type.GetElementType();
            if (left.Type != listType)
                left = Expression.Convert(left, listType);

            var body = Expression.Call(typeof(Enumerable), "Contains", new Type[] { left.Type }, list, left);
            return Expression.TryCatch(body, Expression.Catch(typeof(Exception), Expression.Constant(false)));
        }

        internal override Expression CreateExpression(ParameterExpression sourceData, ParameterExpression param)
        {
            return CreateInExpression(CreateChildExpression(sourceData, param, 2), CreateChildExpression(sourceData, param, 0));
        }
    }
}
