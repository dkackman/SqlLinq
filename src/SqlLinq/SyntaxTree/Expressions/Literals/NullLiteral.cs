using System;
using System.Reflection;

namespace SqlLinq.SyntaxTree.Expressions.Literals
{
    [SyntaxNode(RuleConstants.RULE_VALUE_NULL)]
    public class NullLiteral : LiteralNode
    {
        public NullLiteral() : base(typeof(object))
        {
        }

        internal override object Value => null;

        internal override MethodInfo GetCoercionMethod(Type from)
        {
            return new Func<object, object>(Identity).Method;
        }

        private static object Identity(object o)
        {
            return o;
        }
    }
}
