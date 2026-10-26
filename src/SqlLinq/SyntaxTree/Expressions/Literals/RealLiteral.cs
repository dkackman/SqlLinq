using System;
using System.Reflection;
using System.Globalization;

namespace SqlLinq.SyntaxTree.Expressions.Literals
{
    [SyntaxNode(RuleConstants.RULE_VALUE_REALLITERAL)]
    public class RealLiteral : LiteralNode
    {
        public RealLiteral() : base(typeof(double))
        {
        }

        internal override MethodInfo GetCoercionMethod(Type from)
        {
            return new Func<string, double>(Parse).Method;
        }

        private static double Parse(string s)
        {
            return double.Parse(s, CultureInfo.InvariantCulture);
        }
    }
}
