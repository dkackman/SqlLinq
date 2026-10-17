using System.Linq;

using SqlLinq.SyntaxTree.Joins;

namespace SqlLinq.SyntaxTree.Clauses
{
    [SyntaxNode(RuleConstants.RULE_FROMCLAUSE_FROM)]
    public class FromClause : NonTerminalNode
    {
        public FromClause()
        {
        }

        public string TableName
        {
            get
            {
                return GetTerminalText("Id");
            }
        }

        public Join Join
        {
            get
            {
                return FindDescendants<Join>().FirstOrDefault();
            }
        }
    }
}