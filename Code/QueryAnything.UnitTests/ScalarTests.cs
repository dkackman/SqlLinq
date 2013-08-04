using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using LinqStatistics;

namespace SqlLinq.UnitTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ScalarTests
    {
        public ScalarTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestMethod]
        public void Sum()
        {
            IEnumerable<int> source = TestData.GetInts();
            int result = source.QueryScalar<int>("SELECT sum(value()) FROM this");

            Assert.IsTrue(result == source.Sum());
        }

        [TestMethod]
        public void AvgInt()
        {
            IEnumerable<int> source = TestData.GetInts();
            double result = source.QueryScalar<int, double>("SELECT avg(value()) FROM this");

            Assert.IsTrue(result == source.Average());
        }

        [TestMethod]
        public void AvgDouble()
        {
            IEnumerable<double> source = TestData.GetDoubles();
            double result = source.QueryScalar<double>("SELECT avg(value()) FROM this");

            Assert.IsTrue(result == source.Average());
        }

        [TestMethod]
        public void AvgFloat()
        {
            IEnumerable<float> source = TestData.GetFloats();
            float result = source.QueryScalar<float>("SELECT avg(value()) FROM this");

            Assert.IsTrue(result == source.Average());
        }

        [TestMethod]
        public void AvgProperty()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            double result = source.QueryScalar<Person, double>("SELECT avg(AGE) FROM this");

            Assert.IsTrue(result == source.Average(p => p.Age));
        }

        [TestMethod]
        public void VarProperty()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            double result = source.QueryScalar<Person, double>("SELECT var(age) FROM this");

            Assert.IsTrue(result == source.Variance(p => p.Age));
        }

        [TestMethod]
        public void Count()
        {
            IEnumerable<int> source = TestData.GetInts();
            int result = source.QueryScalar<int>("SELECT count(*) FROM this");

            Assert.IsTrue(result == source.Count());
        }

        [TestMethod]
        public void CountDistinctValue()
        {
            IEnumerable<int> source = TestData.GetInts();
            int result = source.QueryScalar<int>("SELECT count(DISTINCT value()) FROM this");

            Assert.IsTrue(result == source.Distinct().Count());
        }

        [TestMethod]
        public void CountDistinctIgnoresNullValues()
        {
            var source = TestData.GetNullableInts();
            var nonNUllSource = source.Where(i => i.HasValue).Select(i => (int)i);
            int result = nonNUllSource.QueryScalar<int>("SELECT count(DISTINCT value()) FROM this");
            int nullResults = source.QueryScalar<int?, int>("SELECT count(DISTINCT value()) FROM this");

            Assert.IsTrue(result == nullResults);
        }

        [TestMethod]
        public void CountDistinct()
        {
            var source = TestData.GetPeople();
            int result = source.QueryScalar<Person, int>("SELECT count(DISTINCT address) FROM this");

            Assert.IsTrue(result == source.Select(p => p.Address).Where(s => s != null).Distinct().Count());
        }

        [TestMethod]
        public void CountDistinctIgnoresNullProperties()
        {
            var source = TestData.GetPeople();
            int distinct = source.QueryScalar<Person, int>("SELECT count(DISTINCT address) FROM this");
            int notnull = source.QueryScalar<Person, int>("SELECT count(DISTINCT address) FROM this WHERE address IS NOT NULL");

            Assert.IsTrue(distinct == notnull);
        }

        [TestMethod]
        public void CountDistinctWhere()
        {
            var source = TestData.GetPeople();
            int result = source.QueryScalar<Person, int>("SELECT count(DISTINCT address) FROM this WHERE age > 40");

            Assert.IsTrue(result == source.Where(p => p.Age > 40).Select(p => p.Address).Where(s => s != null).Distinct().Count());
        }

        [TestMethod]
        public void CountAllWhere()
        {
            var source = TestData.GetPeople();
            int result = source.QueryScalar<Person, int>("SELECT count(ALL address) FROM this WHERE age > 40");
            int answer = source.Where(p => p.Age > 40).Where(p => p.Address != null).Count();
            Assert.IsTrue(result == answer);
        }

        [TestMethod]
        public void CountAll()
        {
            var source = TestData.GetPeople();
            int result = source.QueryScalar<Person, int>("SELECT count(ALL address) FROM this");

            Assert.IsTrue(result == source.Where(p => p.Address != null).Count());
        }

        [TestMethod]
        public void CountAllValue()
        {
            var source = TestData.GetNullableInts();
            int result = source.QueryScalar<int?, int>("SELECT count(ALL value()) FROM this");

            Assert.IsTrue(result == source.Where(i => i.HasValue).Count());
        }

        [TestMethod]
        public void CountWhere()
        {
            IEnumerable<int> source = TestData.GetInts();
            int result = source.QueryScalar<int>("SELECT count(*) FROM this WHERE value() > 3");

            Assert.IsTrue(result == source.Where(i => i > 3).Count());
        }
    }
}
