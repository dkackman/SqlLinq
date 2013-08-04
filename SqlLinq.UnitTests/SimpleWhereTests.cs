using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlLinq.UnitTests
{
    [TestClass]
    public class SimpleWhereTests
    {
        [TestMethod]
        public void Where()
        {
            List<string> l = new List<string>();
            l.Add("don");
            l.Add("donald");
            l.Add("pihllip");

            IEnumerable<string> result = l.Query<string>("SELECT * FROM this WHERE Length > 3");
            Assert.IsTrue(result.Count() == 2);
        }

        [TestMethod]
        public void WhereValue()
        {
            IEnumerable<int> source = TestData.GetInts();
            IEnumerable<int> result = source.Query<int>("SELECT * FROM this WHERE value() > 3");

            Assert.IsTrue(result.SequenceEqual(source.Where(i => i > 3)));
        }

        [TestMethod]
        public void WhereProperty()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            IEnumerable<Person> result = source.Query<Person>("SELECT * FROM this WHERE age > 40");

            Assert.IsTrue(result.SequenceEqual(source.Where(p => p.Age > 40)));
        }

        [TestMethod]
        public void WherePropertyNotEqualC()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            IEnumerable<Person> result = source.Query<Person>("SELECT * FROM this WHERE age != 40");

            Assert.IsTrue(result.SequenceEqual(source.Where(p => p.Age != 40)));
        }

        [TestMethod]
        public void WherePropertyNotEqualVB()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            IEnumerable<Person> result = source.Query<Person>("SELECT * FROM this WHERE age <> 40");

            Assert.IsTrue(result.SequenceEqual(source.Where(p => p.Age != 40)));
        }

        [TestMethod]
        public void PropertyWhere()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            IEnumerable<int> result = source.Query<Person, int>("SELECT Age FROM this WHERE age > 40");

            Assert.IsTrue(result.SequenceEqual(source.Where(p => p.Age > 40).Select<Person, int>(p => p.Age)));
        }

        [TestMethod]
        public void SelectOnePropertyWhereOtherProperty()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            IEnumerable<int> result = source.Query<Person, int>("SELECT Age FROM this WHERE name = 'Frank'");

            Assert.IsTrue(result.SequenceEqual(source.Where(p => p.Name == "Frank").Select<Person, int>(p => p.Age)));
        }

        [TestMethod]
        public void SelectOnePropertyWhereOtherProperty2()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            int result = source.QueryScalar<Person, int>("SELECT Age FROM this WHERE name = 'Frank'");

            Assert.IsTrue(result == source.Where(p => p.Name == "Frank").Select<Person, int>(p => p.Age).First());
        }
    }
}
