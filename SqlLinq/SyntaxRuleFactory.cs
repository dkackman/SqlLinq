using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using GoldParser;

using SqlLinq.SyntaxTree;

namespace SqlLinq
{
    static class SyntaxRuleFactory
    {
        private static Dictionary<int, Type> _implTypes = LoadImplTypes();

        public static NonTerminalNode CreateNode(Rule rule)
        {
            Debug.Assert(rule != null);

            NonTerminalNode node = _implTypes.TryGetValue(rule.Index, out Type type) ? (NonTerminalNode)Activator.CreateInstance(type) : new NonTerminalNode();
            node.Rule = rule;
            return node;
        }

        private static Dictionary<int, Type> LoadImplTypes()
        {
            return (
                from t in typeof(NonTerminalNode).Assembly.GetTypes()
                where typeof(NonTerminalNode).IsAssignableFrom(t) && !t.IsAbstract
                from SyntaxNodeAttribute attr in t.GetCustomAttributes(typeof(SyntaxNodeAttribute), false)
                select new { Rule = (int)attr.RuleConstant, Type = t }).ToDictionary(x => x.Rule, x => x.Type);
        }
    }
}
