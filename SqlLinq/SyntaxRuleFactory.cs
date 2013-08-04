using System.Linq;
using System.Diagnostics;

using GoldParser;

using Kackman.RuntimeTypeLoader;

using SqlLinq.SyntaxTree;

namespace SqlLinq
{
    static class SyntaxRuleFactory
    {
        private static TypeLoader<NonTerminalNode, int> _implTypes = LoadImplTypes();

        public static NonTerminalNode CreateNode(Rule rule)
        {
            Debug.Assert(rule != null);

            NonTerminalNode node = _implTypes.ContainsKey(rule.Index) ? _implTypes.CreateInstance(rule.Index) : new NonTerminalNode();
            node.Rule = rule;
            return node;
        }

        private static TypeLoader<NonTerminalNode, int> LoadImplTypes()
        {
            TypeLoader<NonTerminalNode, int> loader = new TypeLoader<NonTerminalNode, int>();
            loader.SearchDirectories = false;
            loader.LoadMany(t => t.GetCustomAttributes(typeof(SyntaxNodeAttribute), false).Select(attr => (int)((SyntaxNodeAttribute)attr).RuleConstant));

            return loader;
        }
    }
}
