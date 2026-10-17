using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlLinq.UnitTests
{
    [TestClass]
    public class NullableTests
    {
        [TestMethod]
        public void WhereNullableIntIsNullEquality()
        {
            var source = TestData.GetNullables();
            var result = source.Query<NullableProperty>("SELECT * FROM this WHERE NullableInt = 1");

            Assert.IsTrue(result.SequenceEqual(source.Where(i => i.NullableInt == 1)));
        }

        [TestMethod]
        public void WhereNullableIntIsNullGreaterThanEqual()
        {
            var source = TestData.GetNullables();
            var result = source.Query<NullableProperty>("SELECT * FROM this WHERE NullableInt >= 1");

            Assert.IsTrue(result.SequenceEqual(source.Where(i => i.NullableInt >= 1)));
        }

        [TestMethod]
        public void WhereNullableIntIsNullBetween()
        {
            var source = TestData.GetNullables();
            var result = source.Query<NullableProperty>("SELECT * FROM this WHERE NullableInt BETWEEN 0 AND 4");

            Assert.IsTrue(result.SequenceEqual(source.Where(i => i.NullableInt >= 0 && i.NullableInt <= 4)));
        }

        [TestMethod]
        public void WhereNullableIntIsNullIn()
        {
            var source = TestData.GetNullables();
            var result = source.Query<NullableProperty>("SELECT * FROM this WHERE NullableInt IN( 0, 4 )");

            Assert.IsTrue(result.SequenceEqual(source.Where(i => i.NullableInt == 0 || i.NullableInt == 4)));
        }
    }
}
