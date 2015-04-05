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
            row.Add("name", "don");
            row.Add("age", 42);
            row.Add("zip", 55116);
            data.Add(row);

            row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            row.Add("name", "john");
            row.Add("age", 41);
            row.Add("zip", 55114);
            data.Add(row);

            return data;
        }

        private static IEnumerable<IDictionary<string, int>> GetTypedData()
        {
            List<IDictionary<string, int>> data = new List<IDictionary<string, int>>();
            Dictionary<string, int> row = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            row.Add("id", 1);
            row.Add("value", 2);
            row.Add("value1", 2);
            data.Add(row);

            row = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            row.Add("id", 1);
            row.Add("value", 3);
            row.Add("value1", 3);
            data.Add(row);

            row = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            row.Add("id", 2);
            row.Add("value", 4);
            row.Add("value1", 4);
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
                         where (string)row["name"] == "don"
                         select row;

            var result = data.Query("SELECT * FROM this WHERE name = 'don'");

            Assert.IsTrue(answer.SequenceEqual(result));
        }

        [TestMethod]
        public void SelectOneFieldFromDictionary()
        {
            var data = GetData();

            var answer = from row in data
                         select (string)row["name"];

            var result = data.Query<IDictionary<string, object>, string>("SELECT name FROM this");

            Assert.IsTrue(answer.SequenceEqual(result));
        }

        [TestMethod]
        public void SelectOneFieldFromTypedDictionary()
        {
            var data = GetTypedData();

            var answer = from row in data
                         select (int)row["id"];

            var result = data.Query<IDictionary<string, int>, int>("SELECT id FROM this");

            Assert.IsTrue(answer.SequenceEqual(result));
        }

        [TestMethod]
        public void SelectScalarSumOnDictionary()
        {
            var data = GetTypedData();

            var answer = (from row in data
                          select row["value"]).Sum();

            var result = data.QueryScalar<IDictionary<string, int>, int>("SELECT sum(value) FROM this");

            Assert.AreEqual(answer, result);
        }

        [TestMethod]
        public void SelectGroupOnDictionary()
        {
            var data = GetTypedData();

            var answer = from row in data
                         group row by row["id"] into g
                         select new Tuple<int, int>(g.Key, g.Sum(r => r["value"]));

            var result = data.Query<IDictionary<string, int>, Tuple<int, int>>("SELECT id, sum(value) FROM this group by id");

            Assert.IsTrue(answer.SequenceEqual(result));
        }

        [TestMethod]
        public void SelectOneFieldFromTypedDictionaryWhere()
        {
            var data = GetTypedData();

            var answer = from row in data
                         where row["id"] == 4
                         select row["value"];

            var result = data.Query<IDictionary<string, int>, int>("SELECT value FROM this WHERE id = 4");

            Assert.IsTrue(answer.SequenceEqual(result));
        }

        [TestMethod]
        public void SelectOneFieldFromDictionaryWhere()
        {
            var data = GetData();

            var answer = from row in data
                         where (int)row["age"] == 42
                         select (string)row["name"];

            var result = data.Query<IDictionary<string, object>, string>("SELECT name FROM this WHERE age = 42");

            Assert.IsTrue(answer.SequenceEqual(result));
        }

        [TestMethod]
        public void SelectDictionaryFromDictionary()
        {
            var data = GetData();

            var answer = from row in data
                         select new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                            {
                                {"name", row["name"]},
                                {"zip", row["zip"]}
                            };

            var result = data.Query("SELECT name, zip FROM this");

            Assert.IsTrue(answer.SequenceEqual(result, new DictionaryComparer<string, object>()));
        }

        [TestMethod]
        public void SelectTypedDictionaryFromTypedDictionary()
        {
            var data = GetTypedData();

            var answer = from row in data
                         select new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                            {
                                {"id", row["id"]},
                                {"value1", row["value1"]}
                            };

            var result = data.Query<IDictionary<string, int>, Dictionary<string, int>>("SELECT id, value1 FROM this");

            Assert.IsTrue(answer.SequenceEqual(result, new DictionaryComparer<string, int>()));
        }

        [TestMethod]
        public void SelectDistinctFromDictionary()
        {
            var data = GetData();

            IEnumerable<int> answer = (from row in data
                                       select row["age"]).Distinct().Cast<int>();

            var result = data.Query<IDictionary<string, object>, int>("SELECT DISTINCT age FROM this");

            Assert.IsTrue(answer.SequenceEqual(result));
        }

        [TestMethod]
        public void SelectDistinctFromTypedDictionary()
        {
            var data = GetTypedData();

            IEnumerable<int> answer = (from row in data
                                       select row["value"]).Distinct();

            var result = data.Query<IDictionary<string, int>, int>("SELECT DISTINCT value FROM this");

            Assert.IsTrue(answer.SequenceEqual(result));
        }
    }
}
