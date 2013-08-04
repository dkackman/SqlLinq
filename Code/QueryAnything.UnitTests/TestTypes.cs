using System;
using System.Linq;
using System.Reflection;

namespace SqlLinq.UnitTests
{
    /// <summary>
    /// The SequenceEquals extension method will use Equals by default
    /// This base class just provides a boilerplate equals operator that does
    /// some type checking and then compares hash codes
    /// </summary>
    abstract class EquatableObject
    {
        public override int GetHashCode()
        {
            var values = from p in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                         where p.CanRead
                         select p.GetValue(this, null)
                             into value
                             where value != null
                             select value;
            unchecked
            {
                return values.Aggregate(0, (total, next) => total ^= next != null ? next.GetHashCode() : 0);
            }
        }

        public override bool Equals(object o)
        {
            if (object.ReferenceEquals(o, null) || o.GetType() != this.GetType())
                return false;

            return object.ReferenceEquals(o, this) || this.GetHashCode() == o.GetHashCode();
        }
    }

    class BasicPerson : EquatableObject
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }

    class Person : BasicPerson
    {
        public int Age
        {
            get
            {
                return (int)((DateTime.Now - Birthdate).TotalDays / 365.25);
            }
        }

        public int Weight { get; set; }
        public DateTime Birthdate { get; set; }
    }

    class NullableProperty : EquatableObject
    {
        public int? NullableInt { get; set; }
    }

    class Family : EquatableObject
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public double AverageAge { get; set; }
        public int TotalAge { get; set; }
    }

    class FamilyMember : EquatableObject
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Location { get; set; }
        public int FamilySize { get; set; }
    }

    class Pet : EquatableObject
    {
        public string Name { get; set; }
        public BasicPerson Owner { get; set; }
    }

    class Pair : EquatableObject
    {
        public string OwnerName { get; set; }
        public string PetName { get; set; }
    }
}
