using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlLinq.UnitTests
{
    [TestClass]
    public class TupleTests
    {
        public TupleTests()
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
        public void NestedExpressions()
        {
            IEnumerable<int> source = TestData.GetInts();
            var result = source.Query<int>("SELECT * FROM this WHERE value() = ((6 / 2) + 1 - 2)");
            var answer = source.Where(i => i == ((6 / 2) + 1 - 2));

            Assert.IsTrue(result.Any());
            Assert.IsTrue(answer.SequenceEqual(result));
        }

        [TestMethod]
        public void ExpressionOnFields()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            var result = source.Query<Person, string>("SELECT name FROM this WHERE weight > (age * 2)");
            var answer = source.Where(p => p.Weight > p.Age * 2).Select(p => p.Name);

            Assert.IsTrue(result.Any());
            Assert.IsTrue(answer.SequenceEqual(result));
        }

        [TestMethod]
        public void ExpressionOnFields2()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            var result = source.Query<Person, string>("SELECT name FROM this WHERE (weight / 2)  > (age * 2)");
            var answer = source.Where(p => p.Weight / 2 > p.Age * 2).Select(p => p.Name);

            Assert.IsTrue(result.Any());
            Assert.IsTrue(answer.SequenceEqual(result));
        }
    }
}
