using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlLinq.UnitTests
{
    [TestClass]
    public class NonASCIITests
    {
        [TestMethod]
        public void LikeNonASCIICharacter()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            IEnumerable<Person> result = source.Query<Person>("SELECT * FROM this WHERE name LIKE 'Ø%'");
            var answer = source.Where(p => p.Name.StartsWith("Ø"));

            Assert.IsTrue(result.SequenceEqual(answer));
        }
    }
}
