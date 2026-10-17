using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SqlLinq.SyntaxTree.Expressions.Predicates.Comparison;

namespace SqlLinq.UnitTests
{
    [TestClass]
    public class LikeConversionTests
    {
        [TestMethod]
        public void RegexEndsWithConversion()
        {
            string regex = LikeNode.ConvertLikeToRegex("%def");

            Assert.IsTrue(LikeNode.IsRegexMatch("abcdef", regex));
            Assert.IsFalse(LikeNode.IsRegexMatch("abcdef123", regex));
            Assert.IsFalse(LikeNode.IsRegexMatch("def123", regex));
        }

        [TestMethod]
        public void RegexBeginsWithConversion()
        {
            string regex = LikeNode.ConvertLikeToRegex("abc%");

            Assert.IsTrue(LikeNode.IsRegexMatch("abcdef", regex));
            Assert.IsFalse(LikeNode.IsRegexMatch("123abcdef", regex));
            Assert.IsFalse(LikeNode.IsRegexMatch("123abc", regex));
        }

        [TestMethod]
        public void RegexNoWildcard()
        {
            string regex = LikeNode.ConvertLikeToRegex("bcd");

            Assert.IsFalse(LikeNode.IsRegexMatch("abcdef", regex));
        }

        [TestMethod]
        public void RegexContains()
        {
            string regex = LikeNode.ConvertLikeToRegex("%bcd%");

            Assert.IsTrue(LikeNode.IsRegexMatch("abcdef", regex));
            Assert.IsTrue(LikeNode.IsRegexMatch("bcdef", regex));
            Assert.IsTrue(LikeNode.IsRegexMatch("abcd", regex));
            Assert.IsTrue(LikeNode.IsRegexMatch("bcd", regex));
            Assert.IsFalse(LikeNode.IsRegexMatch("def", regex));
        }

        [TestMethod]
        public void RegexPercentLiteral()
        {
            string regex = LikeNode.ConvertLikeToRegex("e[%]");

            Assert.IsTrue(LikeNode.IsRegexMatch("e%", regex));
            Assert.IsFalse(LikeNode.IsRegexMatch("e", regex));
            Assert.IsFalse(LikeNode.IsRegexMatch("%", regex));
        }

        [TestMethod]
        public void RegexPercentLiteral2()
        {
            string regex = LikeNode.ConvertLikeToRegex("%e[%]");

            Assert.IsTrue(LikeNode.IsRegexMatch("be%", regex));
            Assert.IsTrue(LikeNode.IsRegexMatch("e%", regex));
            Assert.IsFalse(LikeNode.IsRegexMatch("e%d", regex));
        }

        [TestMethod]
        public void RegexSingleCharacterWildcard()
        {
            string regex = LikeNode.ConvertLikeToRegex("b_n");

            Assert.IsTrue(LikeNode.IsRegexMatch("bin", regex));
            Assert.IsTrue(LikeNode.IsRegexMatch("bun", regex));
            Assert.IsFalse(LikeNode.IsRegexMatch("bid", regex));
        }

        [TestMethod]
        public void RegexSingleCharacterWildcardLiteral()
        {
            string regex = LikeNode.ConvertLikeToRegex("b[_]n");

            Assert.IsTrue(LikeNode.IsRegexMatch("b_n", regex));
            Assert.IsFalse(LikeNode.IsRegexMatch("bin", regex));
        }
    }
}
