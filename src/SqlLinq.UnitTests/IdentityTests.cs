using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlLinq.UnitTests
{
    [TestClass]
    public class IdentityTests
    {
        [TestMethod]
        public void SelectIdentityValueType()
        {
            var source = TestData.GetInts();
            var result = source.Query<int>("SELECT * FROM this");

            Assert.IsTrue(result.SequenceEqual(source));
        }

        [TestMethod]
        public void SelectIdentityReferenceType()
        {
            var source = TestData.GetPeople();
            var result = source.Query<Person>("SELECT * FROM this");

            Assert.IsTrue(result.SequenceEqual(source));
        }
    }
}
