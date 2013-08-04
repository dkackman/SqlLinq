using System.Text;
using System.Xml.Linq;

namespace SqlLinq.SyntaxTree
{
    public abstract class SyntaxNode
    {
        public NonTerminalNode Parent { get; internal set; }
        public int Index { get; internal set; }

        protected SyntaxNode()
        {
        }

        private string _treeId;
        public string TreeId
        {
            get
            {
                if (_treeId == null)
                {
                    StringBuilder builder = new StringBuilder();
                    GetTreeId(builder);
                    _treeId = builder.ToString();
                }

                return _treeId;
            }
        }

        protected void GetTreeId(StringBuilder builder)
        {
            if (Parent != null)
            {
                Parent.GetTreeId(builder);
                builder.AppendFormat(":{0}:", Index);
            }
            else
            {
                builder.AppendFormat("root");
            }
        }

        public abstract XElement ToXml();
    }
}
