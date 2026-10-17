using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlLinq.UnitTests
{
    [TestClass]
    public class QueryTests
    {
        [TestMethod]
        public void CompiledQueryEvaluatesEachSource()
        {
            // Arrange
            var query = new Query<IDictionary<string, object>, IDictionary<string, object>>("SELECT * FROM this WHERE lastName = 'Smith'");
            query.Compile();

            var source = TestData.GetPeopleDictionary();
            var firstThree = source.Take(3);

            // Act
            var result = query.Evaluate(source);
            var firstThreeResult = firstThree.Query(query);

            // Assert
            Assert.IsTrue(result.SequenceEqual(source.Where(row => (string)row["lastName"] == "Smith")));
            Assert.IsTrue(firstThreeResult.SequenceEqual(firstThree.Where(row => (string)row["lastName"] == "Smith")));
        }

        [TestMethod]
        public void CompiledScalarQueryEvaluatesEachSource()
        {
            // Arrange
            var query = new ScalarQuery<Person, int>("SELECT max(weight) FROM this");
            query.Compile();

            var source = TestData.GetPeople();
            var firstThree = source.Take(3);

            // Act
            var result = query.Evaluate(source);
            var firstThreeResult = firstThree.QueryScalar(query);

            // Assert
            Assert.AreEqual(source.Max(p => p.Weight), result);
            Assert.AreEqual(firstThree.Max(p => p.Weight), firstThreeResult);
        }
    }
}
