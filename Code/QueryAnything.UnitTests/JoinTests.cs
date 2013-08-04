using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlLinq.UnitTests
{
    [TestClass]
    public class JoinTests
    {
        [TestMethod]
        public void JoinObjects()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            IEnumerable<Family> families = TestData.GetFamilies();

            var answer = from p in source
                         join f in families on p.Address equals f.Address
                         select new FamilyMember { Name = p.Name, LastName = f.Name, Location = p.Address };

            var result = source.Query<Person, Family, FamilyMember>("SELECT Name, that.Name AS LastName, Address AS Location FROM this INNER JOIN that ON this.Address = that.Address", families);

            Assert.IsTrue(result.Any());
            Assert.IsTrue(answer.Any());
            Assert.IsTrue(result.SequenceEqual(answer));
        }

        [TestMethod]
        public void JoinAndGroup()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            IEnumerable<Family> families = TestData.GetFamilies();

            var answer = from p in source
                         join f in families on p.Address equals f.Address
                         group f by f.Name into g
                         select new Tuple<string, int>(g.Key, g.Count());

            var result = source.Query<Person, Family, Tuple<string, int>>("SELECT that.Name, COUNT(*) FROM this INNER JOIN that ON this.Address = that.Address GROUP BY that.Name", families);

            Assert.IsTrue(result.Any());
            Assert.IsTrue(answer.Any());
            Assert.IsTrue(result.SequenceEqual(answer));
        }

        [TestMethod]
        public void JoinAndGroupAndOrder()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            IEnumerable<Family> families = TestData.GetFamilies();

            var answer = from p in source
                         join f in families on p.Address equals f.Address
                         group f by f.Name into g
                         orderby g.Key descending
                         select new Tuple<string, int>(g.Key, g.Count());
                         

            var result = source.Query<Person, Family, Tuple<string, int>>("SELECT that.Name, COUNT(*) FROM this INNER JOIN that ON this.Address = that.Address GROUP BY that.Name ORDER BY that.Name DESC", families);

            Assert.IsTrue(result.Any());
            Assert.IsTrue(answer.Any());
            Assert.IsTrue(result.SequenceEqual(answer));
        }
        [TestMethod]
        public void JoinObjectsIntoDictionary()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            IEnumerable<Family> families = TestData.GetFamilies();

            var answer = from p in source
                         join f in families on p.Address equals f.Address
                         select new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                         { 
                            {"Name", p.Name},
                            {"LastName", f.Name},
                            {"Location", p.Address}
                         };

            var result = source.Query<Person, Family, IDictionary<string, object>>("SELECT Name, that.Name AS LastName, Address AS Location FROM this INNER JOIN that ON this.Address = that.Address", families);

            Assert.IsTrue(result.Any());
            Assert.IsTrue(answer.Any());
            Assert.IsTrue(result.SequenceEqual(answer, new DictionaryComparer<string, object>()));
        }

        [TestMethod]
        public void JoinDictionaries()
        {
            IEnumerable<IDictionary<string, object>> outer = TestData.GetPeopleDictionary();
            IEnumerable<IDictionary<string, object>> inner = TestData.GetFamiliesDictionary();

            var answer = from p in outer
                         join f in inner on p["lastName"] equals f["familyName"]
                         select new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) 
                         {
                             {"Name", p["name"] },            
                             {"lastName", p["lastName"] },
                             {"city", f["city"] }
                         };

            var result = outer.Query<IDictionary<string, object>, IDictionary<string, object>, IDictionary<string, object>>("SELECT Name, lastName, city FROM this INNER JOIN that ON this.lastName = that.familyName", inner);
           
            Assert.IsTrue(result.Any());
            Assert.IsTrue(answer.Any());
            Assert.IsTrue(result.SequenceEqual(answer, new DictionaryComparer<string, object>()));
        }

        [TestMethod]
        public void JoinDictionariesIntoObject()
        {
            IEnumerable<IDictionary<string, object>> outer = TestData.GetPeopleDictionary();
            IEnumerable<IDictionary<string, object>> inner = TestData.GetFamiliesDictionary();

            var answer = from p in outer
                         join f in inner on p["lastName"] equals f["familyName"]
                         select new FamilyMember { Name = p["name"].ToString(), LastName = f["familyName"].ToString(), Location = f["city"].ToString() };


            var result = outer.Query<IDictionary<string, object>, IDictionary<string, object>, FamilyMember>("SELECT Name, lastName, city AS Location FROM this INNER JOIN that ON this.lastName = that.familyName", inner);

            Assert.IsTrue(result.Any());
            Assert.IsTrue(answer.Any());
            Assert.IsTrue(result.SequenceEqual(answer));
        }

        [TestMethod]
        public void JoinDictionariesIntoTuple()
        {
            IEnumerable<IDictionary<string, object>> outer = TestData.GetPeopleDictionary();
            IEnumerable<IDictionary<string, object>> inner = TestData.GetFamiliesDictionary();

            var answer = from p in outer
                         join f in inner on p["lastName"] equals f["familyName"]
                         select new Tuple<string, string, string>(p["name"].ToString(), f["familyName"].ToString(), f["city"].ToString());


            var result = outer.Query<IDictionary<string, object>, IDictionary<string, object>, Tuple<string, string, string>>("SELECT Name, lastName, city AS Location FROM this INNER JOIN that ON this.lastName = that.familyName", inner);

            Assert.IsTrue(result.Any());
            Assert.IsTrue(answer.Any());
            Assert.IsTrue(result.SequenceEqual(answer));
        }

        [TestMethod]
        public void JoinDictionaryToObjects()
        {
            IEnumerable<IDictionary<string, object>> outer = TestData.GetPeopleDictionary();

            IEnumerable<Family> inner = TestData.GetFamilies();

            var answer = from p in outer
                         join f in inner on p["lastName"] equals f.Name
                         select new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) 
                         {
                             {"Name", p["name"] },            
                             {"lastName", p["lastName"] },
                             {"Address", f.Address }
                         };

            var result = outer.Query<IDictionary<string, object>, Family, IDictionary<string, object>>("SELECT Name, lastName, Address FROM this INNER JOIN that ON this.lastName = that.Name", inner);
            
            Assert.IsTrue(result.Any());
            Assert.IsTrue(answer.Any());
            Assert.IsTrue(result.SequenceEqual(answer, new DictionaryComparer<string, object>()));
        }

        [TestMethod]
        public void JoinObjectsToDictionary()
        {
            IEnumerable<Family> outer = TestData.GetFamilies();
            IEnumerable<IDictionary<string, object>> inner = TestData.GetPeopleDictionary();

            var answer = from f in outer
                         join p in inner on f.Name equals p["lastName"]
                         select new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) 
                         {
                             {"Name", p["name"] },            
                             {"lastName", p["lastName"] },
                             {"Address", f.Address }
                         };

            var result = outer.Query<Family, IDictionary<string, object>, IDictionary<string, object>>("SELECT that.Name, that.lastName, Address FROM this INNER JOIN that ON this.Name = that.lastName", inner);

            Assert.IsTrue(result.Any());
            Assert.IsTrue(answer.Any()); 
            Assert.IsTrue(result.SequenceEqual(answer, new DictionaryComparer<string, object>()));
        }

        [TestMethod]
        public void OrderedJoin()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            IEnumerable<Family> families = TestData.GetFamilies();

            var answer = source.Join<Person, Family, string, FamilyMember>(families, p => p.Address, f => f.Address,
                (p, f) => new FamilyMember { Name = p.Name, LastName = f.Name, Location = f.Address }).OrderByDescending(m => m.Location);

            var result = source.Query<Person, Family, FamilyMember>("SELECT Name, that.Name AS LastName, Address AS Location FROM this INNER JOIN that ON this.Address = that.Address ORDER BY Address DESC", families);

            Assert.IsTrue(result.Any());
            Assert.IsTrue(answer.Any());
            Assert.IsTrue(result.SequenceEqual(answer));
        }

        [TestMethod]
        public void JoinWithWhere()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            IEnumerable<Family> families = TestData.GetFamilies();
            var answer = from p in source
                    join f in families on p.Address equals f.Address
                    where p.Address == "401 Main St., St. Paul, MN 55132"
                    select new FamilyMember { Name = p.Name, LastName = f.Name, Location = f.Address };

            var result = source.Query<Person, Family, FamilyMember>("SELECT this.Name, that.Name AS LastName, Address AS Location FROM this INNER JOIN that ON this.Address = that.Address WHERE this.Address = '401 Main St., St. Paul, MN 55132'", families);

            Assert.IsTrue(result.Any());
            Assert.IsTrue(answer.Any());
            Assert.IsTrue(result.SequenceEqual(answer));
        }

        [TestMethod]
        public void JoinWithInnerWhere2()
        {
            IEnumerable<Person> source = TestData.GetPeople();
            IEnumerable<Family> families = TestData.GetFamilies();
            var answer = from p in source
                         join f in families on p.Address equals f.Address
                         where f.Name == "Smith"
                         select new FamilyMember { Name = p.Name, LastName = f.Name, Location = f.Address };

            var result = source.Query<Person, Family, FamilyMember>("SELECT Name, that.Name AS LastName, Address AS Location FROM this INNER JOIN that ON this.Address = that.Address WHERE that.Name = 'Smith'", families);

            Assert.IsTrue(answer.Any());       
            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.SequenceEqual(answer));
        }

        [TestMethod]
        public void JoinWithThisAsKey()
        {
            BasicPerson magnus = new BasicPerson { Name = "Hedlund, Magnus" };
            BasicPerson terry = new BasicPerson { Name = "Adams, Terry" };
            BasicPerson charlotte = new BasicPerson { Name = "Weiss, Charlotte" };

            Pet barley = new Pet { Name = "Barley", Owner = terry };
            Pet boots = new Pet { Name = "Boots", Owner = terry };
            Pet whiskers = new Pet { Name = "Whiskers", Owner = charlotte };
            Pet daisy = new Pet { Name = "Daisy", Owner = magnus };

            List<BasicPerson> people = new List<BasicPerson> { magnus, terry, charlotte };
            List<Pet> pets = new List<Pet> { barley, boots, whiskers, daisy };

            // Create a list of Person-Pet pairs where 
            // each element is an anonymous type that contains a
            // Pet's name and the name of the Person that owns the Pet.
            var answer =
                people.Join<BasicPerson, Pet, BasicPerson, Pair>(pets,
                            person => person,
                            pet => pet.Owner,
                            (person, pet) =>
                                new Pair { OwnerName = person.Name, PetName = pet.Name });

            var result = people.Query<BasicPerson, Pet, Pair>("SELECT this.Name AS OwnerName, that.Name AS PetName FROM this INNER JOIN that ON this = that.Owner", pets);

            Assert.IsTrue(result.Any());
            Assert.IsTrue(answer.Any());
            Assert.IsTrue(result.SequenceEqual(answer));
        }


        [TestMethod]
        public void JoinWithThisAsKeyTuple()
        {
            BasicPerson magnus = new BasicPerson { Name = "Hedlund, Magnus" };
            BasicPerson terry = new BasicPerson { Name = "Adams, Terry" };
            BasicPerson charlotte = new BasicPerson { Name = "Weiss, Charlotte" };

            Pet barley = new Pet { Name = "Barley", Owner = terry };
            Pet boots = new Pet { Name = "Boots", Owner = terry };
            Pet whiskers = new Pet { Name = "Whiskers", Owner = charlotte };
            Pet daisy = new Pet { Name = "Daisy", Owner = magnus };

            List<BasicPerson> people = new List<BasicPerson> { magnus, terry, charlotte };
            List<Pet> pets = new List<Pet> { barley, boots, whiskers, daisy };

            // Create a list of Person-Pet pairs where 
            // each element is an anonymous type that contains a
            // Pet's name and the name of the Person that owns the Pet.
            var answer =
                people.Join<BasicPerson, Pet, BasicPerson, Tuple<string, string>>(pets,
                            person => person,
                            pet => pet.Owner,
                            (person, pet) =>
                                new Tuple<string, string>(person.Name, pet.Name));

            var result = people.Query<BasicPerson, Pet, Tuple<string, string>>("SELECT this.Name AS OwnerName, that.Name AS PetName FROM this INNER JOIN that ON this = that.Owner", pets);

            Assert.IsTrue(result.Any());
            Assert.IsTrue(answer.Any());
            Assert.IsTrue(result.SequenceEqual(answer));
        }
    }
}
