using System;
using System.Reflection;

namespace SqlLinq.SyntaxTree.Expressions.Literals
{
    [SyntaxNode(RuleConstants.RULE_VALUE_STRINGLITERAL)]
    public class StringLiteral : LiteralNode
    {
        public StringLiteral()
            : base(typeof(string))
        {
        }

        internal override MethodInfo GetCoercionMethod(Type from)
        {
            return typeof(Convert).GetMethod("ToString", new Type[] { from });
        }

        internal override object Value
        {
            get
            {
                // to get the value of the string literal need to remove quotes
                return GetCoercionMethod(typeof(string)).Invoke(null, new object[] { GetTerminalText(GetType().Name).Trim('\'') });
            }
        }
    }
}
