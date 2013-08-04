using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlLinq
{
    class ParseException : ApplicationException
    {
        public ParseException()
        {

        }

        public ParseException(string message)
            : base(message)
        {

        }

        public int LinePosition
        {
            get;
            internal set;
        }

        public int LineNumber
        {
            get;
            internal set;
        }
    }
}
