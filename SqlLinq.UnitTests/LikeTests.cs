using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace SqlLinq.UnitTests
{
    [TestClass]
    public class LikeTests
    {
        [TestMethod]
        public void LikeStartsWith()
        {
            var source = TestData.GetPeople();
            var result = source.Query<Person>("SELECT * FROM this WHERE Name LIKE 'e%'");
            var answer = source.Where(p => p.Name.StartsWith("e", StringComparison.OrdinalIgnoreCase));

            Assert.IsTrue(result.SequenceEqual(answer));
        }

        [TestMethod]
        public void LikeEndsWith()
        {
            var source = TestData.GetPeople();
            var result = source.Query<Person>("SELECT * FROM this WHERE Name LIKE '%e'");
            var answer = source.Where(p => p.Name.EndsWith("e", StringComparison.OrdinalIgnoreCase));

            Assert.IsTrue(result.SequenceEqual(answer));
        }

        [TestMethod]
        public void LikeContains()
        {
            var source = TestData.GetPeople();
            var result = source.Query<Person>("SELECT * FROM this WHERE Name LIKE '%e%'");
            var answer = source.Where(p => p.Name.IndexOf("e", StringComparison.OrdinalIgnoreCase) > -1);

            Assert.IsTrue(result.SequenceEqual(answer));
        }
    }
}
