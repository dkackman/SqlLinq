using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlLinq;

namespace SqlLinq.UnitTests
{
    /// <summary>
    /// Summary description for ParseExceptions
    /// </summary>
    [TestClass]
    public class ParseExceptionTests
    {
        public ParseExceptionTests()
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
        [ExpectedException(typeof(SqlException))]
        public void GroupByWithoutSelectingKey()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            var result = source.Query<Person, IDictionary<string, object>>("SELECT Name, Avg(Age) AS AverageAge FROM this GROUP BY Address");
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void SelecFieldtWithoutGroupingOnIt()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            var result = source.Query<Person, IDictionary<string, object>>("SELECT Name, Address, Avg(Age) AS AverageAge FROM this GROUP BY Address");
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void AggregateWithoutGroupBy()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            var result = source.Query<Person, IDictionary<string, object>>("SELECT Name, Count(*) FROM this");
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void GroupByWithoutAggregate()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            var result = source.Query<Person, IDictionary<string, object>>("SELECT Address FROM this GROUP BY Address");
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void WhereClauseWithAggregate()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            var result = source.Query<Person, IDictionary<string, object>>("SELECT Age FROM this WHERE Avg(Age) > 40");
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void DistinctQueryWithOrderByFieldNotInSelectList()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            var result = source.Query<Person, IDictionary<string, object>>("SELECT DISTINCT Name FROM this ORDER BY Age");
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void GroupByQueryWithOrderByFieldNotInSelectList()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            var result = source.Query<Person, IDictionary<string, object>>("SELECT Address, Avg(age) as A FROM this GROUP BY Address ORDER BY Name");
        }
    }
}
