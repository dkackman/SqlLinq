using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlLinq.UnitTests
{
    /// <summary>
    /// Summary description for TypeComparisonTests
    /// </summary>
    [TestClass]
    public class TypeComparisonTests
    {
        public TypeComparisonTests()
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
        public void DateInWhere()
        {
            var source = TestData.GetPeople();
            var result = source.Query<Person>("SELECT * FROM this WHERE Birthdate = #1/2/1905#");
            var answer = from t in source
                         where t.Birthdate == new DateTime(1905, 1, 2)
                         select t;

            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.SequenceEqual(answer));
        }

        [TestMethod]
        public void DateInWhereBetween()
        {
            var source = TestData.GetPeople();
            var result = source.Query<Person>("SELECT * FROM this WHERE Birthdate BETWEEN #1/1/1905# AND #1/1/1975#");
            var answer = from t in source
                         where t.Birthdate >= new DateTime(1905, 1, 1) && t.Birthdate <= new DateTime(1975, 1, 1)
                         select t;

            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.SequenceEqual(answer));
        }

        [TestMethod]
        public void DateInWhereNotBetween()
        {
            var source = TestData.GetPeople();
            var result = source.Query<Person>("SELECT * FROM this WHERE Birthdate NOT BETWEEN #1/3/1905# AND #1/1/1975#");
            var answer = from t in source
                         where !(t.Birthdate >= new DateTime(1905, 1, 3) && t.Birthdate <= new DateTime(1975, 1, 1))
                         select t;

            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.SequenceEqual(answer));
        }

        [TestMethod]
        public void CompareDoubleToInt()
        {
            var source = TestData.GetDoubleIntData();
            var result = source.Query<Tuple<double, int>>("SELECT * FROM this WHERE item1 > item2");
            var answer = from t in source
                         where t.Item1 > t.Item2
                         select t;

            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.SequenceEqual(answer));
        }

        [TestMethod]
        public void CompareIntToDouble()
        {
            var source = TestData.GetDoubleIntData();
            var result = source.Query<Tuple<double, int>>("SELECT * FROM this WHERE item2 > item1");
            var answer = from t in source
                         where t.Item2 > t.Item1
                         select t;

            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.SequenceEqual(answer));
        }

        [TestMethod]
        public void AddIntToDouble()
        {
            var source = TestData.GetDoubleIntData();
            var result = source.Query<Tuple<double, int>>("SELECT * FROM this WHERE item2 = item1 + item2");
            var answer = from t in source
                         where t.Item2 == t.Item1 + t.Item2
                         select t;

            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.SequenceEqual(answer));
        }

        [TestMethod]
        public void AddDoubleToInt()
        {
            var source = TestData.GetDoubleIntData();
            var result = source.Query<Tuple<double, int>>("SELECT * FROM this WHERE item2 = item2 + item1");
            var answer = from t in source
                         where t.Item2 == t.Item2 + t.Item1
                         select t;

            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.SequenceEqual(answer));
        }

        [TestMethod]
        public void IntBetweenDoubleAndInt()
        {
            var source = TestData.GetDoubleIntData();
            var result = source.Query<Tuple<double, int>>("SELECT * FROM this WHERE item2 BETWEEN 1.1 AND 4");
            var answer = from t in source
                         where t.Item2 >= 1.1 && t.Item2 <= 4
                         select t;

            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.SequenceEqual(answer));
        }

        [TestMethod]
        public void IntBetweenIntAndDouble()
        {
            var source = TestData.GetDoubleIntData();
            var result = source.Query<Tuple<double, int>>("SELECT * FROM this WHERE item2 BETWEEN 1 AND 4.2");
            var answer = from t in source
                         where t.Item2 >= 1 && t.Item2 <= 4.2
                         select t;

            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.SequenceEqual(answer));
        }


        [TestMethod]
        public void DoubleBetweenDoubleAndInt()
        {
            var source = TestData.GetDoubleIntData();
            var result = source.Query<Tuple<double, int>>("SELECT * FROM this WHERE item1 BETWEEN 1.1 AND 4");
            var answer = from t in source
                         where t.Item1 >= 1.1 && t.Item1 <= 4
                         select t;

            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.SequenceEqual(answer));
        }

        [TestMethod]
        public void DoubleBetweenIntAndDouble()
        {
            var source = TestData.GetDoubleIntData();
            var result = source.Query<Tuple<double, int>>("SELECT * FROM this WHERE item1 BETWEEN 1 AND 4.2");
            var answer = from t in source
                         where t.Item1 >= 1 && t.Item1 <= 4.2
                         select t;

            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.SequenceEqual(answer));
        }


        [TestMethod]
        public void DoubleBetweenIntAndDoubleWithNegate()
        {
            var source = TestData.GetDoubleIntData();
            var result = source.Query<Tuple<double, int>>("SELECT * FROM this WHERE -item1 BETWEEN 1 AND 4.2");
            var answer = from t in source
                         where -t.Item1 >= 1 && -t.Item1 <= 4.2
                         select t;

            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.SequenceEqual(answer));
        }
    }
}
