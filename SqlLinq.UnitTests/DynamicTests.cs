using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlLinq.UnitTests
{
    [TestClass]
    public class DynamicTests
    {
        [TestMethod]
        public void DynamicOneProperty()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            var result = source.Query<Person, dynamic>("SELECT address FROM this");
            Assert.IsTrue(result.SequenceEqual(source.Select(p => p.Address)));
        }

        [TestMethod]
        public void DynamicTwoProperties()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            var result = source.Query<Person, dynamic>("SELECT age, address FROM this");

            var answer = from p in source
                         select new { p.Age, p.Address };

            IEnumerable<int> ages = result.Select<dynamic, int>(x => x.age);
            IEnumerable<int> ages1 = answer.Select(x => x.Age);
            Assert.IsTrue(ages.SequenceEqual(ages1));

            IEnumerable<string> addresses = result.Select<dynamic, string>(x => x.address);
            IEnumerable<string> addresses1 = answer.Select(x => x.Address);
            Assert.IsTrue(addresses.SequenceEqual(addresses1));
        }

        [TestMethod]
        public void NewObjectTwoProperties()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            var result = source.Query<Person, BasicPerson>("SELECT name, address FROM this");

            var answer = from p in source select new BasicPerson { Name = p.Name, Address = p.Address };

            Assert.IsTrue(result.SequenceEqual(answer));
        }

        [TestMethod]
        public void NewObjectTwoPropertiesWithAs()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            var result = source.Query<Person, FamilyMember>("SELECT name, address AS location FROM this");

            var answer = from p in source select new FamilyMember { Name = p.Name, Location = p.Address };

            Assert.IsTrue(result.SequenceEqual(answer));
        }

        [TestMethod]
        public void NewObjectTwoPropertiesConstructor()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            var result = source.Query<Person, Tuple<string, string>>("SELECT name, address FROM this");

            var answer = from p in source select new Tuple<string, string>(p.Name, p.Address);

            Assert.IsTrue(result.SequenceEqual(answer));
        }

        [TestMethod]
        public void NewObjectThreePropertiesConstructor()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            var result = source.Query<Person, Tuple<string, int, string>>("SELECT name, age, address FROM this");

            var answer = from p in source select new Tuple<string, int, string>(p.Name, p.Age, p.Address);

            Assert.IsTrue(result.SequenceEqual(answer));
        }

        [TestMethod]
        public void ExpandoAsSource()
        {
            var customers = new List<dynamic>();
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

                    customers.Add(customer);
                }
            }

            // all this casting is just to get the answer and result into the same storage types for comparison
            var result = customers.Cast<IDictionary<string, object>>().Query<IDictionary<string, object>, dynamic>("SELECT City, Name FROM this").Cast<IDictionary<string, object>>();
            var answer = customers.Select<dynamic, dynamic>(d => { dynamic o = new ExpandoObject(); o.City = d.City; o.Name = d.Name; return o; }).Cast<IDictionary<string, object>>();

            Assert.IsTrue(result.Any());
            Assert.IsTrue(answer.SequenceEqual(result, new DictionaryComparer<string, object>()));
        }

        [TestMethod]
        public void ExpandoAsSourceWithoutDictionaryCast()
        {
            var customers = new List<dynamic>();
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

                    customers.Add(customer);
                }
            }

            // all this casting is just to get the answer and result into the same storage types for comparison
            var result = customers.Query<dynamic, dynamic>("SELECT City, Name FROM this");
            var answer = customers.Select<dynamic, dynamic>(d => { dynamic o = new ExpandoObject(); o.City = d.City; o.Name = d.Name; return o; });

            Assert.IsTrue(result.Any());
            Assert.AreEqual(answer.Count(), result.Count());

            // can't fiqure out how to get SequenceEqual to compile - brute force
            var comparer = new ExpandoComparer();
            int x = 0;
            foreach (var a in answer)
            {
                var r = result.Skip(x).First();
                Assert.IsTrue(comparer.Equals(a, r));
                x++;
            }
        }

        [TestMethod]
        public void SumOnExpando()
        {
            var customers = new List<dynamic>();
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

                    customers.Add(customer);
                }
            }

            var answer = from c in customers
                         group c by c.City into g
                         select new Tuple<string, int>(g.Key, g.Sum(c => c.Id));

            var result = customers.Query<dynamic, Tuple<string, int>>("SELECT City, sum(Id) FROM this group by City");

            Assert.IsTrue(result.Any());
            Assert.IsTrue(answer.SequenceEqual(result));
        }

        [TestMethod]
        public void ExpandoAsJoin()
        {
            var customers = new List<dynamic>();
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

                    customers.Add(customer);
                }
            }

            var customers2 = new List<dynamic>(customers);

            var result = customers2.Cast<IDictionary<string, object>>().Query<IDictionary<string, object>, IDictionary<string, object>, dynamic>("SELECT this.City FROM this INNER JOIN that ON this.Id = that.Id", customers.Cast<IDictionary<string, object>>());

            var answer = from c2 in customers2
                         join c in customers on c2.Id equals c.Id
                         select c2.City;

            Assert.IsTrue(result.Any());
            Assert.IsTrue(answer.SequenceEqual(result));
        }
    }
}
