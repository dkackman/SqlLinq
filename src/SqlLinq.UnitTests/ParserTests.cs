using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlLinq.UnitTests
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void SyntaxErrorReportsLineAndPosition()
        {
            // Arrange
            var parser = new SqlParser();

            // Act
            var parsed = parser.Parse("SELECT name\nFROM this\nWHERE weight 150 AND name = 'Bortus'");

            // Assert
            Assert.IsFalse(parsed);
            Assert.IsTrue(parser.ErrorString.StartsWith("Syntax Error", StringComparison.Ordinal));
            Assert.AreEqual(3, parser.LineNumber);
            Assert.AreEqual("WHERE weight 150", parser.ErrorLine);
            Assert.AreEqual(17, parser.LinePosition);
        }

        [TestMethod]
        public void LexicalErrorIsReported()
        {
            // Arrange
            var parser = new SqlParser();

            // Act
            var parsed = parser.Parse("SELECT * FROM this WHERE weight > @x");

            // Assert
            Assert.IsFalse(parsed);
            Assert.IsTrue(parser.ErrorString.StartsWith("Lexical Error", StringComparison.Ordinal));
        }

        [TestMethod]
        public void CommentsAndLineBreaksAreIgnored()
        {
            // Arrange
            var source = TestData.GetPeople();
            var answer = source.Where(p => p.Weight > 150).Select(p => p.Name);

            // Act
            var result = source.Query<Person, string>("-- heavy people\r\nSELECT name /* first name */\r\nFROM this\n\tWHERE weight > 150 -- pounds\n");

            // Assert
            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.SequenceEqual(answer));
        }
    }
}
