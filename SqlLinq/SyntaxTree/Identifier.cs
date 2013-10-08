using System.Diagnostics;

namespace SqlLinq.SyntaxTree
{
    public class Identifier
    {
        public Identifier(string id)
        {
            Debug.Assert(id != null);
            LookupId = id;
        }

        /// <summary>
        /// The identifier as present in the source text
        /// </summary>
        public string LookupId { get; private set; }

        /// <summary>
        /// In a qualified identifier in the format table.field is the left string
        /// In a non qualified identifier is empty string
        /// </summary>
        public string Qualifier
        {
            get
            {
                if (LookupId.Contains("."))
                    return LookupId.Substring(0, LookupId.IndexOf("."));

                return "";
            }
        }

        /// <summary>
        /// In a qualified identifier in the format table.field is the right string
        /// In a non qualified identifier is SourceId string
        /// </summary>
        public string LocalId
        {
            get
            {
                if (LookupId.Contains("."))
                    return LookupId.Substring(LookupId.IndexOf(".") + 1);

                return LookupId;
            }
        }

        public override string ToString()
        {
            return LookupId;
        }
    }
}
