using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlLinq.TimeSeries;
using UnitsNet.Metadata;

namespace SqlLinq.Units.UnitTests
{
    [TestClass]
    public class UnitsTests
    {
        private static readonly List<Frame> Frames =
        [
            new Frame { Time = 0, CoreTemp = 20, PlasmaPressure = 30, DriveSpin = 800, Scan = 1 },
            new Frame { Time = 1, CoreTemp = 60, PlasmaPressure = 100, DriveSpin = 3000, Scan = 2 },
            new Frame { Time = 2, CoreTemp = 85, PlasmaPressure = 150, DriveSpin = 6500, Scan = 3 },
            new Frame { Time = 3, CoreTemp = 95, PlasmaPressure = 101.3, DriveSpin = 7000, Scan = 4 },
        ];

        private static readonly UnitConversions Units = new UnitConversions(new Frame().GetObjectMetadata(CultureInfo.InvariantCulture).Values);

        private const double PsiPerKpa = 0.14503773773;

        private static IList<IDictionary<string, object>> Run(string nrql)
        {
            return new TimeSeriesQuery<Frame>(nrql, f => TimeSpan.FromSeconds(f.Time), Units.Expand).Evaluate(Frames);
        }

        private static double Max(string expression)
        {
            return (double)Run("SELECT max(" + expression + ") AS M FROM shipslog").Single()["M"];
        }

        private static void Fails(string expression, string message)
        {
            var e = Assert.ThrowsExactly<SqlException>(() => Run("SELECT max(" + expression + ") FROM shipslog"));
            Assert.Contains(message, e.Message);
        }

        [TestMethod]
        public void ColumnConvertsFromItsOwnUnit()
        {
            // Act
            var fahrenheit = Max("convert([coretemp], '°F')");
            var revolutionsPerSecond = Max("convert(DriveSpin, 'r/s')");

            // Assert
            Assert.AreEqual(95 * 1.8 + 32, fahrenheit, 1e-9);
            Assert.AreEqual(7000 / 60.0, revolutionsPerSecond, 1e-9);
        }

        [TestMethod]
        [DataRow("CONVERT")]
        [DataRow("Convert")]
        public void ConvertNameIsCaseInsensitive(string name)
        {
            // Act
            var fahrenheit = Max(name + "(CoreTemp, '°F')");

            // Assert
            Assert.AreEqual(95 * 1.8 + 32, fahrenheit, 1e-9);
        }

        [TestMethod]
        [DataRow("Temperature.DegreeFahrenheit")]
        [DataRow("DegreeFahrenheit")]
        [DataRow("degreefahrenheit")]
        [DataRow("°F")]
        [DataRow("Temperature.°F")]
        public void ColumnUnitsByIdNameOrAbbreviation(string unit)
        {
            // Act
            var fahrenheit = Max("convert(CoreTemp, '" + unit + "')");

            // Assert
            Assert.AreEqual(95 * 1.8 + 32, fahrenheit, 1e-9);
        }

        [TestMethod]
        public void AllowUnitConversionLimitsTheTargets()
        {
            // Act
            var psi = Max("convert(PlasmaPressure, 'psi')");
            var bar = Max("convert(PlasmaPressure, 'bar')");

            // Assert
            Assert.AreEqual(150 * PsiPerKpa, psi, 1e-6);
            Assert.AreEqual(1.5, bar, 1e-12);

            // Act & Assert
            Fails("convert(PlasmaPressure, 'mbar')", "PlasmaPressure (Kilopascal) can't convert to 'mbar'. Its units are kPa, psi, bar.");
        }

        [TestMethod]
        public void ColumnConvertsToItsOwnUnitEvenIfItsConversionsLeaveItOut()
        {
            // Arrange
            var plasmaPressure = new Frame().GetObjectMetadata(CultureInfo.InvariantCulture)["PlasmaPressure"];
            var units = new UnitConversions(new[] { new QuantityMetadata(plasmaPressure.Property, plasmaPressure.Unit, plasmaPressure.Conversions.Skip(1).ToList()) });

            // Act
            var expanded = units.Expand("convert(PlasmaPressure, 'kPa')");

            // Assert
            Assert.AreEqual("((PlasmaPressure) * 1.0 + 0.0)", expanded);
        }

        [TestMethod]
        public void ThreeArgumentsOnAColumnMustStateItsUnit()
        {
            // Act
            var fahrenheit = Max("convert(CoreTemp, '°C', '°F')");

            // Assert
            Assert.AreEqual(95 * 1.8 + 32, fahrenheit, 1e-9);

            // Act & Assert
            Fails("convert(CoreTemp, '°F', '°C')", "CoreTemp is in DegreeCelsius, not °F");
        }

        [TestMethod]
        public void BareValuesMustNameTheirKind()
        {
            // Arrange
            var scan = new QuantityMetadata(typeof(Frame).GetProperty(nameof(Frame.Scan)), null, []);
            var units = new UnitConversions(new[] { scan });

            // Act & Assert
            var e = Assert.ThrowsExactly<SqlException>(() => units.Expand("convert(Scan, 'psi')"));
            Assert.Contains("needs a column with unit metadata", e.Message);
            Fails("convert(CoreTemp * 2, '°F')", "needs a column with unit metadata");
        }

        [TestMethod]
        public void KindIsNeverGuessed()
        {
            // Act & Assert
            Fails("convert(Scan, 'DegreeCelsius', 'DegreeFahrenheit')", "Scan has no unit metadata, so name the kind of 'DegreeCelsius': 'Temperature.DegreeCelsius' or 'TemperatureDelta.DegreeCelsius'.");
        }

        [TestMethod]
        public void BareValueWithAKind()
        {
            // Act
            var psiFromAbbreviation = Max("convert(Scan, 'Pressure.kPa', 'psi')");
            var psiFromName = Max("convert(Scan, 'Pressure.Kilopascal', 'Pressure.psi')");
            var fahrenheit = Max("convert(Scan, 'Temperature.DegreeCelsius', 'DegreeFahrenheit')");
            var fahrenheitDelta = Max("convert(Scan, 'TemperatureDelta.DegreeCelsius', 'DegreeFahrenheit')");

            // Assert
            Assert.AreEqual(4 * PsiPerKpa, psiFromAbbreviation, 1e-9);
            Assert.AreEqual(4 * PsiPerKpa, psiFromName, 1e-9);
            Assert.AreEqual(4 * 1.8 + 32, fahrenheit, 1e-9);
            Assert.AreEqual(4 * 1.8, fahrenheitDelta, 1e-9);
        }

        [TestMethod]
        public void KindAppliesToBothUnits()
        {
            // Act & Assert
            Fails("convert(Scan, 'Pressure.kPa', 'Temperature.°F')", "'Pressure.kPa' is Pressure, not Temperature");
            Fails("convert(Scan, 'Pressure.kPa', '°F')", "'°F' isn't a Pressure unit");
            Fails("convert(Scan, 'Pressure.nonsense', 'psi')", "'nonsense' isn't a Pressure unit");
            Fails("convert(Scan, 'nonsense', 'psi')", "'nonsense' isn't a unit of any kind");
        }

        [TestMethod]
        public void AbbreviationsAreCaseSensitiveAndNamesAreNot()
        {
            // Act
            var millibar = Max("convert(Scan, 'Pressure.bar', 'mbar')");
            var megabar = Max("convert(Scan, 'Pressure.bar', 'Mbar')");
            var milliseconds = Max("convert(Scan, 'duration.second', 'MilliSecond')");

            // Assert
            Assert.AreEqual(4000.0, millibar, 1e-9);
            Assert.AreEqual(4e-6, megabar, 1e-15);
            Assert.AreEqual(4000.0, milliseconds, 1e-9);

            // Act & Assert
            Fails("convert(Scan, 'Pressure.KPA', 'psi')", "isn't a Pressure unit");
        }

        [TestMethod]
        [DataRow("'Pressure.FootOfElevation', 'bar'")]
        [DataRow("'FuelEfficiency.LiterPer100Kilometers', 'KilometerPerLiter'")]
        [DataRow("'Power.Petawatt', 'Femtowatt'")]
        public void ConversionsThatCantBeExpandedAreRejected(string units)
        {
            // Act & Assert
            Fails("convert(Scan, " + units + ")", "Scan, " + units + " isn't a linear conversion");
        }

        [TestMethod]
        public void DictionaryRowsUseTheMetadataTheyAreGiven()
        {
            // Arrange
            var rows = new double?[] { 20, null, 95 }.Select(coreTemp => (IDictionary<string, object>)new Dictionary<string, object> { ["CoreTemp"] = coreTemp }).ToList();
            var query = new TimeSeriesQuery<IDictionary<string, object>>("SELECT count(*) AS N, count(convert(CoreTemp, '°F')) AS F, max(convert(CoreTemp, '°F')) AS Hottest FROM shipslog WHERE convert(CoreTemp, '°F') > 100 OR CoreTemp IS NULL", r => TimeSpan.Zero, Units.Expand);

            // Act
            var row = query.Evaluate(rows).Single();

            // Assert
            Assert.AreEqual(2, row["N"]);
            Assert.AreEqual(1, row["F"]);
            Assert.AreEqual(95 * 1.8 + 32, (double)row["Hottest"], 1e-9);
        }

        [TestMethod]
        public void ExpandWritesInvariantLinearSql()
        {
            // Arrange
            var culture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            try
            {
                // Act
                var comparison = Units.Expand("convert(CoreTemp, '°F') > 150");
                var negativeOffset = Units.Expand("convert(Scan, 'Temperature.K', '°C')");
                var nested = Units.Expand("convert(convert(PlasmaPressure, 'bar'), 'Pressure.bar', 'psi')");
                var unchanged = Units.Expand("no conversions here");

                // Assert
                Assert.AreEqual("((CoreTemp) * 1.8 + 32.0) > 150", comparison);
                Assert.AreEqual("((Scan) * 1.0 + (-273.15))", negativeOffset);
                Assert.AreEqual("((((PlasmaPressure) * 0.01 + 0.0)) * 14.5037737730209 + 0.0)", nested);
                Assert.AreEqual("no conversions here", unchanged);
            }
            finally
            {
                CultureInfo.CurrentCulture = culture;
            }
        }

        [TestMethod]
        [DataRow("convert(CoreTemp)", "convert takes ")]
        [DataRow("convert(CoreTemp, 'a', 'b', 'c')", "convert takes ")]
        [DataRow("convert(CoreTemp, °F)", "Expected a quoted string but found °F")]
        public void BadCallsThrowSqlException(string expression, string message)
        {
            // Act & Assert
            var e = Assert.ThrowsExactly<SqlException>(() => Units.Expand(expression));
            Assert.IsTrue(e.Message.StartsWith(message, StringComparison.Ordinal), e.Message);
        }
    }
}
