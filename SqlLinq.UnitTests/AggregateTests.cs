using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using LinqStatistics;

namespace SqlLinq.UnitTests
{
    [TestClass]
    public class AggregateTests
    {
        [TestMethod]
        public void TwoAggregatesIntoTuple()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            Tuple<double, double> result = source.Query<Person, Tuple<double, double>>("SELECT avg(AGE) AS a, stdev(AGE) AS s FROM this").First();

            Assert.IsTrue(result.Item1 == source.Average(p => p.Age));
            Assert.IsTrue(result.Item2 == source.StandardDeviation(p => p.Age));
        }

        [TestMethod]
        public void ThreeAggregatesIntoTuple()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            Tuple<double, double, int> result = source.Query<Person, Tuple<double, double, int>>("SELECT avg(AGE) AS a, stdev(AGE) AS s, Count(*) FROM this").First();

            Assert.IsTrue(result.Item1 == source.Average(p => p.Age));
            Assert.IsTrue(result.Item2 == source.StandardDeviation(p => p.Age)); 
            Assert.IsTrue(result.Item3 == source.Count());
        }

        [TestMethod]
        public void TwoAggregatesIntoDictionary()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            IDictionary<string, object> result = source.Query<Person, IDictionary<string, object>>("SELECT avg(AGE) AS a, stdev(AGE) AS s FROM this").First();

            Assert.IsTrue((double)result["a"] == source.Average(p => p.Age));
            Assert.IsTrue((double)result["s"] == source.StandardDeviation(p => p.Age));
        }

        [TestMethod]
        public void TwoAggregatesIntoTypedDictionary()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            IDictionary<string, double> result = source.Query<Person, IDictionary<string, double>>("SELECT avg(AGE) AS a, stdev(AGE) AS s FROM this").First();

            Assert.IsTrue(result["a"] == source.Average(p => p.Age));
            Assert.IsTrue(result["s"] == source.StandardDeviation(p => p.Age));
        }

        [TestMethod]
        public void TwoAggregatesIntoExpando()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            var result = source.Query<Person, dynamic>("SELECT avg(AGE) AS a, stdev(AGE) AS s FROM this").First();

            Assert.IsTrue(result.a == source.Average(p => p.Age));
            Assert.IsTrue(result.s == source.StandardDeviation(p => p.Age));
        }

        [TestMethod]
        public void ScalarAggregateIntoObject()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            var result = source.QueryScalar<Person, object>("SELECT avg(AGE) FROM this");

            Assert.IsTrue(result.Equals(source.Average(p => p.Age)));
        }
    }
}
