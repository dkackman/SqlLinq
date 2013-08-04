using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlLinq.UnitTests
{
    [TestClass]
    public class OperatorTests
    {
        public OperatorTests()
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
        public void EmbeddedTrueInWhereClause()
        {
            IEnumerable<int> source = TestData.GetInts();
            var result = source.Query<int>("SELECT * FROM this WHERE value() > 3 AND TRUE");
            var answer = source.Where(i => i > 3);

            Assert.IsTrue(result.Any());
            Assert.IsTrue(answer.SequenceEqual(result));
        }

        [TestMethod]
        public void EmbeddedFalseInWhereClause()
        {
            IEnumerable<int> source = TestData.GetInts();
            var result = source.Query<int>("SELECT * FROM this WHERE value() > 3 AND FALSE");

            Assert.IsTrue(result.Any() == false);
        }

        [TestMethod]
        public void TrueInWhereClause()
        {
            IEnumerable<int> source = TestData.GetInts();
            var result = source.Query<int>("SELECT * FROM this WHERE TRUE");

            Assert.IsTrue(result.Any());
            Assert.IsTrue(source.SequenceEqual(result));
        }

        [TestMethod]
        public void FalseInWhereClause()
        {
            IEnumerable<int> source = TestData.GetInts();
            var result = source.Query<int>("SELECT * FROM this WHERE FALSE");

            Assert.IsTrue(result.Any() == false);
        }

        [TestMethod]
        public void NonZeroEvaluautesAsTrueInWhereClause()
        {
            IEnumerable<int> source = TestData.GetInts();
            var result = source.Query<int>("SELECT * FROM this WHERE 2 + 3");

            Assert.IsTrue(result.Any());
            Assert.IsTrue(source.SequenceEqual(result));
        }

        [TestMethod]
        public void ZeroEvaluautesAsFalseInWhereClause()
        {
            IEnumerable<int> source = TestData.GetInts();
            var result = source.Query<int>("SELECT * FROM this WHERE 2 - 2");

            Assert.IsTrue(result.Any() == false);
        }

        [TestMethod]
        public void WhereIntInValue()
        {
            IEnumerable<int> source = TestData.GetInts();
            var result = source.Query<int>("SELECT * FROM this WHERE value() IN(2, 3)");

            Assert.IsTrue(result.Any());
            foreach (int i in result)
                Assert.IsTrue(i == 2 || i == 3);
        }

        [TestMethod]
        public void WhereInValueDouble()
        {
            IEnumerable<double> source = TestData.GetDoubles();
            var result = source.Query<double>("SELECT * FROM this WHERE value() IN(2.1, 4.6)");
            var answer = from d in source
                         where (new List<double> { 2.1, 4.6 }).Contains(d)
                         select d;

            Assert.IsTrue(result.Any());
            Assert.IsTrue(answer.Any());

            Assert.IsTrue(answer.SequenceEqual(result));
        }

        [TestMethod]
        public void WhereIntNotInValue()
        {
            IEnumerable<int> source = TestData.GetInts();
            var result = source.Query<int>("SELECT * FROM this WHERE value() NOT IN(2, 3)");

            Assert.IsTrue(result.Any());
            foreach (int i in result)
                Assert.IsTrue(i != 2 && i != 3);
        }
    }
}
