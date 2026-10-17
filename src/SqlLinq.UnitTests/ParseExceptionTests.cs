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

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void GroupByWithoutSelectingKey()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            Assert.ThrowsExactly<SqlException>(() => source.Query<Person, IDictionary<string, object>>("SELECT Name, Avg(Age) AS AverageAge FROM this GROUP BY Address"));
        }

        [TestMethod]
        public void SelecFieldtWithoutGroupingOnIt()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            Assert.ThrowsExactly<SqlException>(() => source.Query<Person, IDictionary<string, object>>("SELECT Name, Address, Avg(Age) AS AverageAge FROM this GROUP BY Address"));
        }

        [TestMethod]
        public void AggregateWithoutGroupBy()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            Assert.ThrowsExactly<SqlException>(() => source.Query<Person, IDictionary<string, object>>("SELECT Name, Count(*) FROM this"));
        }

        [TestMethod]
        public void GroupByWithoutAggregate()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            Assert.ThrowsExactly<SqlException>(() => source.Query<Person, IDictionary<string, object>>("SELECT Address FROM this GROUP BY Address"));
        }

        [TestMethod]
        public void WhereClauseWithAggregate()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            Assert.ThrowsExactly<SqlException>(() => source.Query<Person, IDictionary<string, object>>("SELECT Age FROM this WHERE Avg(Age) > 40"));
        }

        [TestMethod]
        public void DistinctQueryWithOrderByFieldNotInSelectList()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            Assert.ThrowsExactly<SqlException>(() => source.Query<Person, IDictionary<string, object>>("SELECT DISTINCT Name FROM this ORDER BY Age"));
        }

        [TestMethod]
        public void GroupByQueryWithOrderByFieldNotInSelectList()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            Assert.ThrowsExactly<SqlException>(() => source.Query<Person, IDictionary<string, object>>("SELECT Address, Avg(age) as A FROM this GROUP BY Address ORDER BY Name"));
        }

        [TestMethod]
        public void LongGroupByExpressionGenerateExpcetion()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            Assert.ThrowsExactly<SqlException>(() => source.Query<Person, IDictionary<string, object>>("SELECT Name1, Name2, Name3, Name4, Name5, Name6, Name7, Name8, Name9, Avg(age) as A FROM this GROUP BY Name1, Name2, Name3, Name4, Name5, Name6, Name7, Name8, Name9"));
        }
    }
}
