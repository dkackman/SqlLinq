using System;
using System.Dynamic;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlLinq.UnitTests
{
    class T
    {
        public int? RevisionStatusId { get; set; }
        public string TaskNr { get; set; }
    }

    [TestClass]
    public class Bugs
    {
        [TestMethod]
        public void IntToStringConversion()
        {
            IEnumerable<T> source = new List<T>()
            {
                new T(){RevisionStatusId=1, TaskNr="2"}
            };
            IEnumerable<T> result = source.Query<T, T>("SELECT * FROM this WHERE  ( (1=1) AND ((RevisionStatusId IS NOT NULL) AND (RevisionStatusId = 2)) )  ORDER BY TaskNr");
            //  Assert.IsTrue(result.SequenceEqual(source.Select(p => p.Age)));
        }
    }
}
