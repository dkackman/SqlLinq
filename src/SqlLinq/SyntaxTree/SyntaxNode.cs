using System.Text;
using System.Xml.Linq;
using System.Globalization;

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
                builder.AppendFormat(CultureInfo.CurrentCulture, ":{0}:", Index);
            }
            else
            {
                builder.Append("root");
            }
        }

        public abstract XElement ToXml();
    }
}
