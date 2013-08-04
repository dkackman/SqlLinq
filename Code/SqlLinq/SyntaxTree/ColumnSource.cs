using System.Collections.Generic;

using SqlLinq.SyntaxTree.Expressions;

namespace SqlLinq.SyntaxTree
{
    [SyntaxNode(RuleConstants.RULE_COLUMNSOURCE_ID2)]  // AS Alias
    [SyntaxNode(RuleConstants.RULE_COLUMNSOURCE_ID)]
    [SyntaxNode(RuleConstants.RULE_COLUMNSOURCE2)]
    public class ColumnSource : NonTerminalNode
    {
        private Identifier m_id;

        public ColumnSource()
        {
        }

        public virtual string Alias
        {
            get
            {
                NodeWithId alias = FindChild<NodeWithId>(RuleConstants.RULE_COLUMNALIAS_AS_ID);
                if (alias != null)
                    return alias.Id.LocalId;

                return Id.LocalId;
            }
        }

        protected virtual Identifier CreateIdentifier()
        {
            return new Identifier(GetTerminalText("Id").Trim('[', ']'));
        }

        public Identifier Id
        {
            get
            {
                if (m_id == null)
                    m_id = CreateIdentifier();

                return m_id;
            }
        }

        internal virtual IEnumerable<Identifier> GetFields()
        {
            yield return this.Id;
        }        
    }
}

