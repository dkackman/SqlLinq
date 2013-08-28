using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlLinq.UnitTests
{
    [TestClass]
    public class DictionaryComparerTests
    {
        [TestMethod]
        public void DictionariesAreEqual()
        {
            var x = new Dictionary<string, object>()
            {
                { "a", 1 }
            };

            var y = new Dictionary<string, object>()
            {
                { "a", 1 }
            };

            var comparer = new DictionaryComparer<string, object>();
            Assert.IsTrue(comparer.Equals(x, y));
            Assert.IsTrue(comparer.GetHashCode(x) == comparer.GetHashCode(y));
        }

        [TestMethod]
        public void DictionariesAreNotEqualBasedOnMemberValue()
        {
            var x = new Dictionary<string, object>()
            {
                { "a", 1 }
            };

            var y = new Dictionary<string, object>()
            {
                { "a", 2 }
            };

            var comparer = new DictionaryComparer<string, object>();
            Assert.IsFalse(comparer.Equals(x, y));
            Assert.IsFalse(comparer.GetHashCode(x) == comparer.GetHashCode(y));
        }

        [TestMethod]
        public void DictionariesAreNotEqualBasedOnMemberCount()
        {
            var x = new Dictionary<string, object>()
            {
                { "a", 1 }
            };

            var y = new Dictionary<string, object>()
            {
                { "a", 2 },
                { "b", 2 }
            };

            var comparer = new DictionaryComparer<string, object>();
            Assert.IsFalse(comparer.Equals(x, y));
            Assert.IsFalse(comparer.GetHashCode(x) == comparer.GetHashCode(y));
        }

        [TestMethod]
        public void DictionaryAreNotEqualBasedOnMemberKey()
        {
            var x = new Dictionary<string, object>()
            {
                { "a", 2 },
                { "c", 2 }
            };

            var y = new Dictionary<string, object>()
            {
                { "a", 2 },
                { "b", 2 }
            };

            var comparer = new DictionaryComparer<string, object>();
            Assert.IsFalse(comparer.Equals(x, y)); 
            Assert.IsFalse(comparer.GetHashCode(x) == comparer.GetHashCode(y));
        }
    }
}
