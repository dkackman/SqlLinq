using System;
using System.Reflection;

namespace SqlLinq.SyntaxTree.Expressions.Literals
{
    [SyntaxNode(RuleConstants.RULE_VALUE_NULL)]
    public class NullLiteral : LiteralNode
    {
        public NullLiteral()
            : base(typeof(object))
        {
        }

        internal override MethodInfo GetCoercionMethod(Type from)
        {
            return new Func<object, object>(o => o).Method;
        }
    }
}
