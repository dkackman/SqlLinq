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

        [TestMethod]
        public void CompareNullExpandoPropertyToBoolean()
        {            
            IEnumerable<ExpandoObject> source = new List<ExpandoObject>()
            {
                new ExpandoObject()
            };
            var result = source.Query<IDictionary<string, object>, dynamic>("SELECT * FROM this WHERE NonExistent = true");
            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void CompareExpandoPropertyToBoolean()
        {
            dynamic one = new ExpandoObject();
            one.BoolProp = true;

            dynamic two = new ExpandoObject();
            two.BoolProp = false;

            dynamic three = new ExpandoObject(); // doesn't have the property

            IList<ExpandoObject> source = new List<ExpandoObject>();
            source.Add((ExpandoObject)one);
            source.Add((ExpandoObject)two);
            source.Add((ExpandoObject)three); 

            var result = source.Query<IDictionary<string, object>, dynamic>("SELECT * FROM this WHERE BoolProp = true");
            Assert.AreEqual(1, result.Count());
            Assert.IsTrue(object.ReferenceEquals(one, result.First()));
        }
    }
}
