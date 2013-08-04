using System;
using System.Reflection;

namespace SqlLinq.SyntaxTree.Expressions.Literals
{
    [SyntaxNode(RuleConstants.RULE_VALUE_INTEGERLITERAL)]
    public class IntegerLiteral : LiteralNode
    {
        public IntegerLiteral()
            : base(IntPtr.Size == 8 ? typeof(Int64) : typeof(Int32))
        {
        }

        internal override MethodInfo GetCoercionMethod(Type from)
        {
            if(IntPtr.Size == 8)
                return typeof(Convert).GetMethod("ToInt64", new Type[] { from });

            return typeof(Convert).GetMethod("ToInt32", new Type[] { from });
        }
    }
}
