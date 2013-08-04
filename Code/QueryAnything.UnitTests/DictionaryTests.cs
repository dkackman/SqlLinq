using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlLinq;

namespace SqlLinq.UnitTests
{
    [TestClass]
    public class DictionaryTests
    {
        private static IEnumerable<IDictionary<string, object>> GetData()
        {
            List<IDictionary<string, object>> data = new List<IDictionary<string, object>>();
            Dictionary<string, object> row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            row.Add("a", 1);
            row.Add("b", 2);
            row.Add("c", 2);
            data.Add(row);

            row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            row.Add("a", 3);
            row.Add("b", 4);
            row.Add("c", 4);
            data.Add(row);

            return data;
        }

        private static IEnumerable<IDictionary<string, int>> GetTypedData()
        {
            List<IDictionary<string, int>> data = new List<IDictionary<string, int>>();
            Dictionary<string, int> row = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            row.Add("a", 1);
            row.Add("b", 2);
            row.Add("c", 2);
            data.Add(row);

            row = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            row.Add("a", 3);
            row.Add("b", 4);
            row.Add("c", 4);
            data.Add(row);

            return data;
        }

        [TestMethod]
        public void DictionaryIdentityTest()
        {
            var data = GetData();
            var result = data.Query("SELECT * FROM this");

            Assert.IsTrue(data.SequenceEqual(result));
        }

        [TestMethod]
        public void DictionaryWhereTest()
        {
            var data = GetData();

            var answer = from row in data
                         where (int)row["a"] == 1
                         select row;

            var result = data.Query("SELECT * FROM this WHERE a = 1");

            Assert.IsTrue(answer.SequenceEqual(result));
        }

        [TestMethod]
        public void SelectOneFieldFromDictionary()
        {
            var data = GetData();

            var answer = from row in data
                         select (int)row["a"];

            var result = data.Query<IDictionary<string, object>, int>("SELECT a FROM this");

            Assert.IsTrue(answer.SequenceEqual(result));
        }

        [TestMethod]
        public void SelectOneFieldFromTypedDictionary()
        {
            var data = GetTypedData();

            var answer = from row in data
                         select (int)row["a"];

            var result = data.Query<IDictionary<string, int>, int>("SELECT a FROM this");

            Assert.IsTrue(answer.SequenceEqual(result));
        }

        [TestMethod]
        public void SelectOneFieldFromTypedDictionaryWhere()
        {
            var data = GetTypedData();

            var answer = from row in data
                         where row["b"] == 4
                         select row["a"];

            var result = data.Query<IDictionary<string, int>, int>("SELECT a FROM this WHERE b = 4");

            Assert.IsTrue(answer.SequenceEqual(result));
        }

        [TestMethod]
        public void SelectOneFieldFromDictionaryWhere()
        {
            var data = GetData();

            var answer = from row in data
                         where (int)row["b"] == 4
                         select (int)row["a"];

            var result = data.Query<IDictionary<string, object>, int>("SELECT a FROM this WHERE b = 4");

            Assert.IsTrue(answer.SequenceEqual(result));
        }

        [TestMethod]
        public void SelectDictionaryFromDictionary()
        {
            var data = GetData();

            var answer = from row in data
                         select new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                            {
                                {"a", row["a"]},
                                {"c", row["c"]}
                            };

            var result = data.Query("SELECT a, c FROM this");

            Assert.IsTrue(answer.SequenceEqual(result, new DictionaryComparer<string, object>()));
        }

        [TestMethod]
        public void SelectTypedDictionaryFromTypedDictionary()
        {
            var data = GetTypedData();

            var answer = from row in data
                         select new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                            {
                                {"a", row["a"]},
                                {"c", row["c"]}
                            };

            var result = data.Query<IDictionary<string, int>, Dictionary<string, int>>("SELECT a, c FROM this");

            Assert.IsTrue(answer.SequenceEqual(result, new DictionaryComparer<string, int>()));
        }

        [TestMethod]
        public void SelectDistinctFromDictionary()
        {
            var data = GetData();

            IEnumerable<int> answer = (from row in data
                                       select row["b"]).Distinct().Cast<int>();

            var result = data.Query<IDictionary<string, object>, int>("SELECT DISTINCT b FROM this");

            Assert.IsTrue(answer.SequenceEqual(result));
        }

        [TestMethod]
        public void SelectDistinctFromTypedDictionary()
        {
            var data = GetTypedData();

            IEnumerable<int> answer = (from row in data
                                       select row["b"]).Distinct();

            var result = data.Query<IDictionary<string, int>, int>("SELECT DISTINCT b FROM this");

            Assert.IsTrue(answer.SequenceEqual(result));
        }
    }
}
