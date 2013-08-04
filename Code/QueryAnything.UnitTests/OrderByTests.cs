using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlLinq.UnitTests
{
    [TestClass]
    public class OrderByTests
    {
        [TestMethod]
        public void OrderByValue()
        {
            IEnumerable<int> source = TestData.GetInts();
            IEnumerable<int> result = source.Query<int>("SELECT * FROM this ORDER BY value()");
            Assert.IsTrue(result.SequenceEqual(source.OrderBy(i => i)));
        }

        [TestMethod]
        public void OrderByProperty()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            IEnumerable<Person> result = source.Query<Person>("SELECT * FROM this ORDER BY age");
            Assert.IsTrue(result.SequenceEqual(source.OrderBy(p => p.Age)));
        }

        [TestMethod]
        public void OrderByTwoProperties()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            IEnumerable<Person> result = source.Query<Person>("SELECT * FROM this ORDER BY age, name");
            IEnumerable<Person> answer = source.OrderBy(p => p.Age).ThenBy(p => p.Name);
            Assert.IsTrue(result.SequenceEqual(answer));
        }

        [TestMethod]
        public void OrderByPropertySelectOtherProperty()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            IEnumerable<string> result = source.Query<Person, string>("SELECT Name FROM this ORDER BY age");
            Assert.IsTrue(result.SequenceEqual(source.OrderBy(p => p.Age).Select(p => p.Name)));
        }

        [TestMethod]
        public void OrderByPropertyDescending()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            IEnumerable<Person> result = source.Query<Person>("SELECT * FROM this ORDER BY age DESC");
            Assert.IsTrue(result.SequenceEqual(source.OrderByDescending(p => p.Age)));
        }
    }
}
