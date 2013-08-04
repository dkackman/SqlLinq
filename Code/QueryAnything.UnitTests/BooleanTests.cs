using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlLinq.UnitTests
{
    /// <summary>
    /// Summary description for BooleanTests
    /// </summary>
    [TestClass]
    public class BooleanTests
    {
        public BooleanTests()
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
        public void SelectWhereTrue()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            IEnumerable<int> result = source.Query<Person, int>("SELECT age FROM this WHERE TRUE");
            Assert.IsTrue(result.SequenceEqual(source.Select(p => p.Age)));
        }

        [TestMethod]
        public void SelectWhereFalse()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            IEnumerable<int> result = source.Query<Person, int>("SELECT age FROM this WHERE FALSE");
            Assert.IsFalse(result.Any());
        }
    }
}
