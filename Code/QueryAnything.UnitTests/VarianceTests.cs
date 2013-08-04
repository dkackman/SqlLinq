﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using LinqStatistics;

namespace SqlLinq.UnitTests
{
    [TestClass]
    public class VarianceTests
    {
        [TestMethod]
        public void VarDouble()
        {
            IEnumerable<double> source = TestData.GetDoubles();

            double result = source.QueryScalar<double>("SELECT var(value()) FROM this");

            Assert.AreEqual(result, 4.1091666666666667, double.Epsilon);
        }

        [TestMethod]
        public void VarNullableDouble()
        {
            IEnumerable<double?> source = TestData.GetNullableDoubles();

            double? result = source.QueryScalar<double?>("SELECT var(value()) FROM this");

            Assert.AreEqual((double)result, 4.1091666666666667, double.Epsilon);
        }

        [TestMethod]
        public void VarInt()
        {
            IEnumerable<int> source = TestData.GetInts();

            double result = source.QueryScalar<int, double>("SELECT var(value()) FROM this");

            Assert.AreEqual(result, source.Variance(), TestData.Epsilon);
        }

        [TestMethod]
        public void VarNullableInt()
        {
            IEnumerable<int?> source = TestData.GetNullableInts();

            double? result = source.QueryScalar<int?, double?>("SELECT var(value()) FROM this");

            Assert.AreEqual((double)result, 2.91666666666666667, double.Epsilon);
        }

        [TestMethod]
        public void VarPDouble()
        {
            IEnumerable<double> source = TestData.GetDoubles();

            double result = source.QueryScalar<double>("SELECT varp(value()) FROM this");

            Assert.AreEqual(result, 3.081875, double.Epsilon);
        }

        [TestMethod]
        public void VarPNullableDouble()
        {
            IEnumerable<double?> source = TestData.GetNullableDoubles();

            double? result = source.QueryScalar<double?>("SELECT varp(value()) FROM this");

            Assert.AreEqual((double)result, 3.081875, double.Epsilon);
        }

        [TestMethod]
        public void VarPInt()
        {
            IEnumerable<int> source = TestData.GetInts();

            double result = source.QueryScalar<int, double>("SELECT varp(value()) FROM this");

            Assert.AreEqual(result, source.VarianceP(), TestData.Epsilon);
        }

        [TestMethod]
        public void VarPNullableInt()
        {
            IEnumerable<int?> source = TestData.GetNullableInts();

            double? result = source.QueryScalar<int?, double?>("SELECT varp(value()) FROM this");

            Assert.AreEqual((double)result, 2.1875, double.Epsilon);
        }
    }
}
