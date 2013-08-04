using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlLinq.UnitTests
{
    static class TestData
    {
        public const double Epsilon = 0.00000000000001;

        public static IEnumerable<NullableProperty> GetNullables()
        {
            return new List<NullableProperty>()
            {
                new NullableProperty(){ NullableInt = 1 },
                new NullableProperty(){ NullableInt = 5 },
                new NullableProperty()
            };
        }

        public static IEnumerable<int> GetInts()
        {
            return new int[] { 4, 2, 3, 6, 6, 4, 120 };
        }

        public static IEnumerable<int?> GetNullableInts()
        {
            return new int?[] { 4, 2, null, 3, 6 };
        }

        public static IEnumerable<double> GetDoubles()
        {
            return new double[] { 4.6, 2.1, 3.5, 6.9 };
        }

        public static IEnumerable<double?> GetNullableDoubles()
        {
            return new double?[] { 4.6, 2.1, 3.5, null, 6.9 };
        }

        public static IEnumerable<float> GetFloats()
        {
            return new float[] { 4.6f, 2.1f, 3.5f, 6.9f };
        }

        public static IEnumerable<Family> GetFamilies()
        {
            return new List<Family>
            {
                new Family { Name = "Smith", Address = "401 Main St., St. Paul, MN 55132"},
                new Family { Name = "Jones", Address = "56 23rd Ave., Minneapolis, MN 55406"},
                new Family { Name = "Johnson", Address = "42 Some Ct., Suburb, MN 55263"},
                new Family { Name = "Chin", Address = "6789 Flower St., St. Paul, MN 55869"}
            };
        }

        public static IEnumerable<Person> GetPeople()
        {
            return new List<Person> 
            { 
                new Person { Birthdate = new DateTime(1905, 1, 2), Weight=101, Name = "Irma", Address = "401 Main St., St. Paul, MN 55132" },            
                new Person { Birthdate = new DateTime(1945, 3, 1), Weight=120, Name = "Jane", Address = "401 Main St., St. Paul, MN 55132" },
                new Person { Birthdate = new DateTime(1970, 2, 21), Weight=170, Name = "Frank", Address = "56 23rd Ave., Minneapolis, MN 55406" },
                new Person { Birthdate = new DateTime(1979, 5, 22), Weight=120, Name = "Louise", Address = "56 23rd Ave., Minneapolis, MN 55406" },
                new Person { Birthdate = new DateTime(1961, 5, 13), Weight=140, Name = "Susan", Address = "42 Some Ct., Suburb, MN 55263" },
                new Person { Birthdate = new DateTime(1965, 10, 29), Weight=185, Name = "Bill", Address = "6789 Flower St., St. Paul, MN 55869" }, 
                new Person { Birthdate = new DateTime(1962, 11, 6), Weight=110, Name = "Betty", Address = "6789 Flower St., St. Paul, MN 55869" }, 
                new Person { Birthdate = new DateTime(1995, 12, 14), Weight=200, Name = "Tim", Address = "42 Some Ct., Suburb, MN 55263" }, 
                new Person { Birthdate = new DateTime(1994, 7, 3), Weight=130, Name = "Megan", Address = "42 Some Ct., Suburb, MN 55263" }, 
                new Person { Birthdate = new DateTime(1932, 7, 21), Weight=160, Name = "Joe", Address = "401 Main St., St. Paul, MN 55132" },
                new Person { Birthdate = new DateTime(1936, 6, 20), Weight=180, Name = "Jim", Address = null },
                new Person { Birthdate = new DateTime(1936, 6, 20), Weight=180, Name = "Øleg", Address = null }
            };
        }

        public static IEnumerable<IDictionary<string, object>> GetPeopleDictionary()
        {
            return new List<Dictionary<string, object>>()
            {
                new Dictionary<string,object>(StringComparer.OrdinalIgnoreCase)
                {
                    {"name", "don"},
                    {"lastName", "Smith"}
                },
                new Dictionary<string,object>(StringComparer.OrdinalIgnoreCase)
                {
                    {"name", "jim"},
                    {"lastName", "Jones"}
                },
                new Dictionary<string,object>(StringComparer.OrdinalIgnoreCase)
                {
                    {"name", "john"},
                    {"lastName", "Johnson"}
                },
                new Dictionary<string,object>(StringComparer.OrdinalIgnoreCase)
                {
                    {"name", "jane"},
                    {"lastName", "Smith"}
                },
                new Dictionary<string,object>(StringComparer.OrdinalIgnoreCase)
                {
                    {"name", "jane"},
                    {"lastName", "Jones"}
                },
                                new Dictionary<string,object>(StringComparer.OrdinalIgnoreCase)
                {
                    {"name", "diane"},
                    {"lastName", "Chin"}
                }
            };
        }

        public static IEnumerable<IDictionary<string, object>> GetFamiliesDictionary()
        {
            return new List<Dictionary<string, object>>()
            {
                new Dictionary<string,object>(StringComparer.OrdinalIgnoreCase)
                {
                    {"city", "Minneapolis"},
                    {"familyName", "Smith"}
                },
                new Dictionary<string,object>(StringComparer.OrdinalIgnoreCase)
                {
                    {"city", "St. Paul"},
                    {"familyName", "Jones"}
                },
                new Dictionary<string,object>(StringComparer.OrdinalIgnoreCase)
                {
                    {"city", "Duluth"},
                    {"familyName", "Johnson"}
                },
            };
        }

        public static IEnumerable<Tuple<double, int>> GetDoubleIntData()
        {
            return new List<Tuple<double, int>>()
            {
                new Tuple<double,int>(1, 1),
                new Tuple<double,int>(10.5, 2),
                new Tuple<double,int>(3.2, 5),
                new Tuple<double,int>(0.5, 6),
                new Tuple<double,int>(2.1, 2),
                new Tuple<double,int>(0, 2),
                new Tuple<double,int>(-2, 2),  
                new Tuple<double,int>(-1.5, 2)  
            };
        }
    }
}
