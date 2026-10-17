using System;
using System.Reflection;

namespace SqlLinq.SyntaxTree.Expressions.Literals
{
    [SyntaxNode(RuleConstants.RULE_VALUE_BOOLEANLITERAL)]
    public class BooleanLiteral : LiteralNode
    {
        public BooleanLiteral()
            : base(typeof(bool))
        {
        }

        internal override MethodInfo GetCoercionMethod(Type from)
        {
            return typeof(Convert).GetMethod("ToBoolean", new Type[] { from });
        }
    }
}
