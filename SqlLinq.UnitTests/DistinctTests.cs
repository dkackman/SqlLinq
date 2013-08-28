using System;
using System.Dynamic;
using System.Linq;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlLinq.UnitTests
{
    [TestClass]
    public class DistinctTests
    {
        [TestMethod]
        public void DistinctValue()
        {
            IEnumerable<int> source = TestData.GetInts();
            IEnumerable<int> result = source.Query<int>("SELECT DISTINCT * FROM this");
            Assert.IsTrue(result.SequenceEqual(source.Distinct()));
        }

        [TestMethod]
        public void DistinctProperty()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            IEnumerable<string> result = source.Query<Person, string>("SELECT DISTINCT address FROM this");
            Assert.IsTrue(result.SequenceEqual(source.Select(p => p.Address).Distinct()));
        }

        [TestMethod]
        public void DistinctMultipleProperties()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            var result = source.Query<Person, Tuple<string, string>>("SELECT DISTINCT name, address FROM this");

            var answer = (from p in source
                          select new Tuple<string, string>(p.Name, p.Address)).Distinct();

            Assert.IsTrue(result.SequenceEqual(answer));
        }

        [TestMethod]
        public void ExpandoAsDistinct()
        {
            var source = new List<dynamic>();
            for (int i = 0; i < 10; ++i)
            {
                string iter = i.ToString();
                for (int j = 0; j < 3; ++j)
                {
                    dynamic customer = new ExpandoObject();
                    customer.City = "Chicago" + iter;
                    customer.Id = i;
                    customer.Name = "Name" + iter;
                    customer.CompanyName = "Company" + iter + j.ToString();

                    source.Add(customer);
                }
            }
            var result = source.Cast<IDictionary<string, object>>().Query<IDictionary<string, object>, dynamic>("SELECT DISTINCT City, Name FROM this").Cast<IDictionary<string, object>>();

            var answer = source.Select(d => { dynamic o = new ExpandoObject(); o.City = d.City; o.Name = d.Name; return o; }).Cast<IDictionary<string, object>>().Distinct(new DictionaryComparer<string, object>());

            Assert.IsTrue(result.Any());
            Assert.IsTrue(answer.SequenceEqual(result, new DictionaryComparer<string, object>()));
        }
    }
}
