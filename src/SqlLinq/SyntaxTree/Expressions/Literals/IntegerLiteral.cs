using System;
using System.Reflection;

namespace SqlLinq.SyntaxTree.Expressions.Literals
{
    [SyntaxNode(RuleConstants.RULE_VALUE_INTEGERLITERAL)]
    public class IntegerLiteral : LiteralNode
    {
        public IntegerLiteral()
            : base(typeof(Int32))
        {
        }

        internal override MethodInfo GetCoercionMethod(Type from)
        {
            return typeof(Convert).GetMethod("ToInt32", new Type[] { from });
        }
    }
}
