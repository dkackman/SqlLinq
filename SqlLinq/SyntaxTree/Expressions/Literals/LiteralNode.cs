using System;
using System.Diagnostics;
using System.Reflection;
using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Expressions.Literals
{
    public abstract class LiteralNode : ExpressionNode
    {
        private Type m_type;

        private LiteralNode()
        {
        }

        protected LiteralNode(Type type)
        {
            Debug.Assert(type != null);
            m_type = type;
        }

        internal override Expression CreateExpression(ParameterExpression sourceData, ParameterExpression param)
        {
            return Expression.Constant(Value, Type);
        }

        public Type Type
        {
            get
            {
                return m_type;
            }
        }

        internal abstract MethodInfo GetCoercionMethod(Type from);

        internal virtual object Value
        {
            get
            {
                return GetCoercionMethod(typeof(string)).Invoke(null, new object[] { GetTerminalText(GetType().Name) });
            }
        }
    }
}
