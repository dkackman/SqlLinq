using System;
using System.Reflection;

namespace SqlLinq.SyntaxTree.Expressions.Literals
{
    [SyntaxNode(RuleConstants.RULE_VALUE_REALLITERAL)]
    public class RealLiteral : LiteralNode
    {
        public RealLiteral()
            : base(typeof(double))
        {
        }

        internal override MethodInfo GetCoercionMethod(Type from)
        {
            return typeof(Convert).GetMethod("ToDouble", new Type[] { from });
        }
    }
}
