using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Diagnostics;

namespace SqlLinq.SyntaxTree.Expressions
{
    [SyntaxNode(RuleConstants.RULE_TUPLE_LPAREN_RPAREN)]
    public class TupleSelectNode : ExpressionNode
    {
        public TupleSelectNode()
        {
        }

        internal override Expression CreateExpression(ParameterExpression sourceData, ParameterExpression param)
        {
            SelectNode select = FindChild<SelectNode>(1);
            Debug.Assert(select != null);
            Debug.Assert(select.IsScalarHint, "record set sub queries don't work yet");

            Type queryType = typeof(ScalarQuery<,>).MakeGenericType(param.Type, typeof(object));

            ICompile query = (ICompile)Activator.CreateInstance(queryType, "subquery");
            query.Compile(select);

            return Expression.Call(Expression.Constant(query), query.GetType().GetMethod("Evaluate"), sourceData);
        }
    }
}
