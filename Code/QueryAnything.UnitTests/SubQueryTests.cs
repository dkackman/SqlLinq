using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlLinq.UnitTests
{
    [TestClass]
    public class SubQueryTests
    {
        [TestMethod]
        [Ignore]
        public void SelectWhereInSubQuery()
        {
            var people = TestData.GetPeople();

            var pp = people.Query("SELECT * FROM this");
            var mm = people.Query<Person, object>("SELECT MAX(age), MIN(age) FROM this");


            var result = people.Query("SELECT * FROM this WHERE age IN ((SELECT MAX(age) FROM this))");

            var answer = from p in people
                         where (from p2 in people
                                 where p2.Age == people.Max(p1 => p1.Age) || p2.Age == people.Min(p1 => p1.Age)
                                 select p2.Age).Contains(p.Age)
                         select p;

            Assert.IsTrue(result.SequenceEqual(answer));
        }

        [TestMethod]
        public void SelectWhereSubquery()
        {
            var people = TestData.GetPeople();

            var result = people.Query("SELECT * FROM this WHERE age > (SELECT AVG(age) FROM this)");

            var answer = from p in people
                         where p.Age > people.Average(p1 => p1.Age)
                         select p;

            Assert.IsTrue(result.SequenceEqual(answer));
        }
    }
}
