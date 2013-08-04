﻿using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;

using GoldParser;

namespace SqlLinq.SyntaxTree
{
    public class NonTerminalNode : SyntaxNode
    {
        private IList<SyntaxNode> m_children = new List<SyntaxNode>();

        public NonTerminalNode()
        {
        }

        internal Rule Rule { get; set; }       

        /// <summary>
        /// This method is used during parsing to enforce syntax rules not expressed in the grammar
        /// </summary>
        internal virtual void CheckSyntax()
        {
        }

        public override string ToString()
        {
            return ToXml().ToString();
        }

        public override XElement ToXml()
        {
            XElement element = new XElement(Rule.Name.Replace("<", "").Replace(">", "").Replace(" ", ""));

            element.Add(new XAttribute("type", GetType().Name));
            element.Add(new XAttribute("ruleId", (RuleConstants)Rule.Index));

            foreach (var n in m_children)
                element.Add(n.ToXml());

            return element;
        }

        internal protected string GetTerminalText(string name)
        {
            TerminalNode terminal = m_children.OfType<TerminalNode>().SingleOrDefault(node => node.Symbol.Name == name);
            return terminal != null ? terminal.Text : string.Empty;
        }

        internal protected IEnumerable<T> FindDescendants<T>() where T : NonTerminalNode
        {
            var list = new List<T>();
            foreach (NonTerminalNode node in m_children.OfType<NonTerminalNode>())
            {
                if (node is T)
                    list.Add(node as T);

                list.AddRange(node.FindDescendants<T>());
            }

            return list;
        }

        internal T FindChild<T>(RuleConstants child) where T : NonTerminalNode
        {
            return m_children.OfType<T>().SingleOrDefault(node => node.Rule.Index == (int)child);
        }

        internal protected T FindChild<T>() where T : SyntaxNode
        {
            return m_children.OfType<T>().SingleOrDefault();
        }

        internal protected T FindChild<T>(int index) where T : SyntaxNode
        {
            if (index > m_children.Count)
                return null;

            return m_children[index] as T;
        }

        internal void AppendChildNode(SyntaxNode node)
        {
            Debug.Assert(node != null);

            m_children.Add(node);
            node.Index = m_children.IndexOf(node);
            node.Parent = this;
        }
    }
}
