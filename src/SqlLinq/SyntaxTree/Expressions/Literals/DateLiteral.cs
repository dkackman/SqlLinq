using System;
using System.Reflection;
using System.Globalization;

namespace SqlLinq.SyntaxTree.Expressions.Literals
{
    [SyntaxNode(RuleConstants.RULE_VALUE_DATELITERAL)]
    public class DateLiteral : LiteralNode
    {
        public DateLiteral()
            : base(typeof(DateTime))
        {
        }

        internal override MethodInfo GetCoercionMethod(Type from)
        {
            if (from == typeof(string))
                return new Func<string, DateTime>(DateTime.Parse).Method;

            return new Func<object, DateTime>(o => DateTime.Parse(o.ToString(), CultureInfo.CurrentCulture)).Method;
        }
    }
}
