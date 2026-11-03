using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlLinq.TimeSeries.UnitTests
{
    [TestClass]
    public class TimeSeriesTests
    {
        private static List<Sample> GetSamples()
        {
            return
            [
                new Sample { Id = 1, Time = 0.0, DriveSpin = 850, PlasmaPressure = 30.5, FuelMix = 14.7, DriveStage = 0, Quantum = false, Note = "Union Point" },
                new Sample { Id = 2, Time = 0.5, DriveSpin = 2500, PlasmaPressure = 55.0, FuelMix = 14.2, DriveStage = 1, Quantum = false, Note = "Moclus" },
                new Sample { Id = 3, Time = 1.0, DriveSpin = 4800, PlasmaPressure = 85.2, FuelMix = 12.9, DriveStage = 2, Quantum = false, Note = "Xelaya" },
                new Sample { Id = 4, Time = 1.5, DriveSpin = 6200, PlasmaPressure = 98.7, FuelMix = 12.1, DriveStage = 2, Quantum = true, Note = "Krill space" },
                new Sample { Id = 5, Time = 2.0, DriveSpin = 7900, PlasmaPressure = 101.3, FuelMix = 11.8, DriveStage = 3, Quantum = true, Note = null },
                new Sample { Id = 6, Time = 2.5, DriveSpin = 3100, PlasmaPressure = 40.0, FuelMix = null, DriveStage = 3, Quantum = false, Note = "Kaylon" },
                new Sample { Id = 7, Time = 3.0, DriveSpin = 5600, PlasmaPressure = 92.0, FuelMix = 12.5, DriveStage = 4, Quantum = true, Note = "" },
                new Sample { Id = 8, Time = 3.5, DriveSpin = 850, PlasmaPressure = 29.9, FuelMix = 14.7, DriveStage = 0, Quantum = false, Note = "Union Point" },
            ];
        }

        private static IList<IDictionary<string, object>> Run(string nrql)
        {
            return new TimeSeriesQuery<Sample>(nrql, s => TimeSpan.FromSeconds(s.Time)).Evaluate(GetSamples());
        }

        private static object[] Column(IEnumerable<IDictionary<string, object>> rows, string name)
        {
            return rows.Select(r => r[name]).ToArray();
        }

        private static Dictionary<string, object> Row(object measured, object ordered)
        {
            return new Dictionary<string, object> { ["Measured"] = measured, ["Ordered"] = ordered };
        }

        [TestMethod]
        public void UnfilteredBucketsReadEachTimestampOnceAndKeepEmptyBuckets()
        {
            // Arrange
            var reads = 0;
            var query = new TimeSeriesQuery<int>(
                "SELECT count(*) AS Samples FROM shipslog TIMESERIES 1 second",
                value => { reads++; return TimeSpan.FromSeconds(value); });
            var source = new[] { 3, 0, 3 };

            // Act
            var result = query.Evaluate(source);

            // Assert
            Assert.AreEqual(3, reads);
            CollectionAssert.AreEqual(new object[] { 1, 0, 0, 2 }, Column(result, "Samples"));
            CollectionAssert.AreEqual(new object[] { TimeSpan.Zero, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3) }, Column(result, "BeginTime"));
        }

        [TestMethod]
        public void NrqlFunctionNames()
        {
            // Arrange
            var driveSpin = GetSamples().Select(s => (double)s.DriveSpin).ToList();
            var expectedAverage = driveSpin.Average();
            var expectedStddev = Math.Sqrt(driveSpin.Sum(r => Math.Pow(r - driveSpin.Average(), 2)) / (driveSpin.Count - 1));

            // Act
            var row = Run("SELECT average(DriveSpin) AS A, stddev(DriveSpin) AS S, uniqueCount(DriveStage) AS U FROM shipslog").Single();

            // Assert
            Assert.AreEqual(expectedAverage, row["A"]);
            Assert.AreEqual(expectedStddev, (double)row["S"], 1e-6);
            Assert.AreEqual(5, row["U"]);
        }

        [TestMethod]
        public void CommasAndParenthesesInsideStringsAndBracketsAreText()
        {
            // Act
            var rows = Run("SELECT count(*) AS [Samples, all] FROM shipslog FACET cases(DriveSpin < 3000 AS 'a) Malloy, Gordon', DriveSpin >= 3000 AS 'b) LaMarr') AS [Helm]");

            // Assert
            CollectionAssert.AreEqual(new object[] { "b) LaMarr", "a) Malloy, Gordon" }, Column(rows, "Helm"));
            CollectionAssert.AreEqual(new object[] { 5, 3 }, Column(rows, "Samples, all"));
        }

        [TestMethod]
        public void TimeseriesBucketsByDuration()
        {
            // Act
            var rows = Run("SELECT max(DriveSpin) AS TopSpin FROM shipslog TIMESERIES 1 second");

            // Assert
            CollectionAssert.AreEqual(new object[] { 2500, 6200, 7900, 5600 }, Column(rows, "TopSpin"));
            CollectionAssert.AreEqual(new object[] { TimeSpan.Zero, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3) }, Column(rows, "BeginTime"));
            CollectionAssert.AreEqual(new object[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(4) }, Column(rows, "EndTime"));
        }

        [TestMethod]
        [DataRow("500 ms", 8)]
        [DataRow("500 milliseconds", 8)]
        [DataRow("1", 4)]
        [DataRow("1.5 s", 3)]
        [DataRow("2 seconds", 2)]
        [DataRow("1 minute", 1)]
        [DataRow("1 h", 1)]
        public void TimeseriesUnits(string duration, int buckets)
        {
            // Act
            var rows = Run("SELECT count(*) AS N FROM shipslog TIMESERIES " + duration);

            // Assert
            Assert.HasCount(buckets, rows);
        }

        [TestMethod]
        public void BucketCountsAddUp()
        {
            // Act
            var rows = Run("SELECT count(*) AS N FROM shipslog TIMESERIES 1.5 s");

            // Assert
            Assert.AreEqual(8, rows.Sum(r => (int)r["N"]));
        }

        [TestMethod]
        public void Facet()
        {
            // Act
            var rows = Run("SELECT count(*) AS N, max(DriveSpin) AS TopSpin FROM shipslog FACET DriveStage");

            // Assert
            Assert.HasCount(5, rows);
            var stageTwo = rows.Single(r => (int)r["DriveStage"] == 2);
            Assert.AreEqual(2, stageTwo["N"]);
            Assert.AreEqual(6200, stageTwo["TopSpin"]);
        }

        [TestMethod]
        public void FacetOnTwoFields()
        {
            // Arrange
            var expectedFacets = GetSamples().Select(s => new { s.DriveStage, s.Quantum }).Distinct().Count();

            // Act
            var rows = Run("SELECT count(*) AS N FROM shipslog FACET DriveStage, Quantum");

            // Assert
            Assert.AreEqual(expectedFacets, rows.Count);
        }

        [TestMethod]
        public void FacetFieldAlsoSelected()
        {
            // Act
            var rows = Run("SELECT DriveStage, count(*) AS N FROM shipslog FACET DriveStage");

            // Assert
            Assert.HasCount(5, rows);
        }

        [TestMethod]
        public void FacetWithTimeseries()
        {
            // Act
            var rows = Run("SELECT max(DriveSpin) AS TopSpin FROM shipslog FACET Quantum TIMESERIES 2 seconds");

            // Assert
            // 0-2 s: ids 1-4 (Quantum on id 4), 2-4 s: ids 5-8 (Quantum on 5 and 7)
            Assert.HasCount(4, rows);
            var late = rows.Where(r => (TimeSpan)r["BeginTime"] == TimeSpan.FromSeconds(2)).ToDictionary(r => (bool)r["Quantum"], r => r["TopSpin"]);
            Assert.AreEqual(7900, late[true]);
            Assert.AreEqual(3100, late[false]);
        }

        [TestMethod]
        public void EmptyBucketsAreFilled()
        {
            // Act
            var rows = Run("SELECT count(*) AS N, average(PlasmaPressure) AS M FROM shipslog WHERE Quantum = TRUE TIMESERIES 1 second");

            // Assert
            CollectionAssert.AreEqual(new object[] { TimeSpan.Zero, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3) }, Column(rows, "BeginTime"));
            CollectionAssert.AreEqual(new object[] { 0, 1, 1, 1 }, Column(rows, "N"));
            CollectionAssert.AreEqual(new object[] { null, 98.7, 101.3, 92.0 }, Column(rows, "M"));
        }

        [TestMethod]
        public void NoMatchingRowsGivesZeroCountAndNullAggregates()
        {
            // Act
            var row = Run("SELECT count(*) AS N, average(DriveSpin) AS A, sum(DriveSpin) AS S FROM shipslog WHERE DriveSpin > 99999").Single();

            // Assert
            Assert.AreEqual(0, row["N"]);
            Assert.IsNull(row["A"]);
            Assert.AreEqual(0.0, row["S"]);
        }

        [TestMethod]
        public void NoMatchingFacetsGiveNoRows()
        {
            // Act
            var rows = Run("SELECT count(*) FROM shipslog WHERE DriveSpin > 99999 FACET DriveStage");

            // Assert
            Assert.IsEmpty(rows);
        }

        [TestMethod]
        public void EmptySourceGivesNoRows()
        {
            // Arrange
            var query = new TimeSeriesQuery<Sample>("SELECT count(*) FROM shipslog", s => TimeSpan.Zero);
            var source = new List<Sample>();

            // Act
            var rows = query.Evaluate(source);

            // Assert
            Assert.IsEmpty(rows);
        }

        [TestMethod]
        public void SinceAndUntilAreAbsoluteOffsets()
        {
            // Act
            var windowCount = Run("SELECT count(*) AS N FROM shipslog SINCE 1 UNTIL 3").Single()["N"];
            var sinceCount = Run("SELECT count(*) AS N FROM shipslog SINCE 1 second").Single()["N"];
            var untilCount = Run("SELECT count(*) AS N FROM shipslog UNTIL 1000 ms").Single()["N"];

            // Assert
            // [1 s, 3 s): ids 3 to 6
            Assert.AreEqual(4, windowCount);
            Assert.AreEqual(6, sinceCount);
            Assert.AreEqual(2, untilCount);
        }

        [TestMethod]
        public void AgoIsRelativeToTheNewestRow()
        {
            // Act
            var sinceAgoCount = Run("SELECT count(*) AS N FROM shipslog SINCE 1 second ago").Single()["N"];
            var untilAgoCount = Run("SELECT count(*) AS N FROM shipslog UNTIL 3 seconds ago").Single()["N"];

            // Assert
            Assert.AreEqual(3, sinceAgoCount);
            Assert.AreEqual(1, untilAgoCount);
        }

        [TestMethod]
        public void ClausesCanComeInAnyOrderAndAnyCase()
        {
            // Arrange
            var expected = Run("SELECT max(DriveSpin) AS TopSpin FROM shipslog WHERE DriveStage > 0 FACET Quantum SINCE 1 TIMESERIES 1 second");

            // Act
            var shuffled = Run("select max(DriveSpin) as TopSpin from shipslog timeseries 1 second\nfacet Quantum since 1 where DriveStage > 0");

            // Assert
            Assert.HasCount(expected.Count, shuffled);
            for (var i = 0; i < expected.Count; i++)
            {
                CollectionAssert.AreEquivalent(expected[i].ToList(), shuffled[i].ToList());
            }
        }

        [TestMethod]
        public void KeywordsMatchInAnyCulture()
        {
            // Arrange
            var culture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            try
            {
                // no other test queries TimeSpan items, so the keyword and COMPARE WITH patterns are built under tr-TR
                var source = new[] { TimeSpan.Zero, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(90), TimeSpan.FromSeconds(100) };
                var query = new TimeSeriesQuery<TimeSpan>("select count(*) as n from shipslog since 60 until 120 timeseries 1 MINUTE compare with 1 minute", t => t);

                // Act
                var rows = query.Evaluate(source);

                // Assert
                CollectionAssert.AreEqual(new object[] { 3, 1 }, Column(rows, "n"));
            }
            finally
            {
                CultureInfo.CurrentCulture = culture;
            }
        }

        [TestMethod]
        public void KeywordsInsideStringsAndBracketsAreIgnored()
        {
            // Arrange
            var source = new[] { new Dictionary<string, object> { { "since", 1 } }, new Dictionary<string, object> { { "since", 5 } } };
            var query = new TimeSeriesQuery<Dictionary<string, object>>("SELECT max([since]) AS M FROM shipslog", d => TimeSpan.Zero);

            // Act
            var stringRow = Run("SELECT count(*) AS N FROM shipslog WHERE Note <> 'since limit facet'").Single();
            var bracketRow = query.Evaluate(source).Single();

            // Assert
            Assert.AreEqual(8, stringRow["N"]);
            Assert.AreEqual(5, bracketRow["M"]);
        }

        [TestMethod]
        public void CompiledQueryRerunsOnLiveData()
        {
            // Arrange
            var query = new TimeSeriesQuery<Sample>("SELECT max(DriveSpin) AS TopSpin FROM shipslog SINCE 1 second ago", s => TimeSpan.FromSeconds(s.Time));
            var live = GetSamples().Take(5).ToList();

            // Act
            var firstTopSpin = query.Evaluate(live).Single()["TopSpin"];
            live.AddRange(GetSamples().Skip(5));
            var secondTopSpin = query.Evaluate(live).Single()["TopSpin"];

            // Assert
            Assert.AreEqual(7900, firstTopSpin);
            Assert.AreEqual(5600, secondTopSpin);
        }

        [TestMethod]
        public void WorksWithDictionaryRows()
        {
            // Arrange
            var source = GetSamples().Select(s => (IDictionary<string, object>)new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) { { "id", s.Id }, { "drivespin", s.DriveSpin }, { "drivestage", s.DriveStage } });
            var query = new TimeSeriesQuery<IDictionary<string, object>>("SELECT max(drivespin) AS TopSpin FROM shipslog FACET drivestage", d => TimeSpan.FromSeconds((int)d["id"]));

            // Act
            var rows = query.Evaluate(source);

            // Assert
            Assert.HasCount(5, rows);
        }

        [TestMethod]
        [DataRow("SELECT count(*) FROM shipslog TIMESERIES 0 seconds", "TIMESERIES ")]
        [DataRow("SELECT count(*) FROM shipslog TIMESERIES fast", "TIMESERIES ")]
        [DataRow("SELECT count(*) FROM shipslog TIMESERIES 1 second ago", "TIMESERIES ")]
        [DataRow("SELECT count(*) FROM shipslog SINCE yesterday", "SINCE ")]
        [DataRow("SELECT count(*) FROM shipslog LIMIT 0", "LIMIT ")]
        [DataRow("SELECT count(*) FROM shipslog TIMESERIES 1 second LIMIT 5", "LIMIT ")]
        [DataRow("SELECT count(*), DriveSpin FROM shipslog", "'DriveSpin' isn't an aggregate")]
        [DataRow("SELECT DriveSpin FROM shipslog TIMESERIES 1 second", "FACET and TIMESERIES ")]
        [DataRow("SELECT DriveSpin FROM shipslog FACET DriveStage", "FACET and TIMESERIES ")]
        [DataRow("SELECT percentile(DriveSpin) FROM shipslog", "percentile takes ")]
        [DataRow("SELECT percentile(DriveSpin, 101) FROM shipslog", "101 isn't a percentile")]
        [DataRow("SELECT percentile(DriveSpin, 5, 95) - min(DriveSpin) FROM shipslog", "percentile takes ")]
        [DataRow("SELECT rate(DriveSpin, 1 second) FROM shipslog", "rate takes ")]
        [DataRow("SELECT rate(count(*), 0 seconds) FROM shipslog", "rate needs ")]
        [DataRow("SELECT average() FROM shipslog", "average takes ")]
        [DataRow("SELECT count(*) FROM shipslog COMPARE WITH 1 second ago", "COMPARE WITH ")]
        [DataRow("SELECT count(*) FROM shipslog SINCE 1 COMPARE 1 second", "COMPARE WITH ")]
        [DataRow("SELECT count(*) FROM shipslog SINCE 1 SINCE 2", "The query has more than one SINCE clause.")]
        [DataRow("SELECT count(*) FROM shipslog GROUP BY DriveStage", "GROUP ")]
        [DataRow("SELECT count(*) FROM shipslog ORDER BY DriveSpin", "ORDER ")]
        [DataRow("SELECT average(Warp) FROM shipslog TIMESERIES 1 second", "SQL compile error:")]
        [DataRow("SELECT FROM shipslog", "A time series query starts SELECT")]
        public void InvalidQueriesThrowSqlException(string nrql, string message)
        {
            // Act & Assert
            var e = Assert.ThrowsExactly<SqlException>(() => new TimeSeriesQuery<Sample>(nrql, s => TimeSpan.FromSeconds(s.Time)));
            Assert.IsTrue(e.Message.StartsWith(message, StringComparison.Ordinal), e.Message);
        }

        [TestMethod]
        public void RewriteSeesEachExpression()
        {
            // Arrange
            var rewrite = (string expression) => expression.Replace("twice(DriveSpin)", "DriveSpin * 2", StringComparison.Ordinal);
            var query = new TimeSeriesQuery<Sample>("SELECT max(twice(DriveSpin)), count(*) AS N FROM shipslog WHERE twice(DriveSpin) > 10000", s => TimeSpan.Zero, rewrite);

            // Act
            var row = query.Evaluate(GetSamples()).Single();

            // Assert
            Assert.AreEqual(15800, row["max(twice(DriveSpin))"]);
            Assert.AreEqual(3, row["N"]);
        }

        [TestMethod]
        public void RewriteGetsEachExpressionOnce()
        {
            // Arrange
            var seen = new List<string>();
            var rewrite = (string expression) =>
            {
                seen.Add(expression);
                return expression;
            };
            var expected = new[] { "Quantum", "DriveSpin", "DriveStage < 2", "DriveStage >= 2", "DriveSpin", "PlasmaPressure", "agg0 - agg1", "FuelMix", "DriveStage > 0", "Id", "PlasmaPressure * 2" };

            // Act
            _ = new TimeSeriesQuery<Sample>("SELECT max(DriveSpin) - min(PlasmaPressure) AS Spread, average(FuelMix) FROM shipslog WHERE DriveStage > 0 FACET Quantum, buckets(DriveSpin, 8000, 4), cases(DriveStage < 2 AS 'Krill', DriveStage >= 2 AS 'Kaylon')", s => TimeSpan.Zero, rewrite);
            _ = new TimeSeriesQuery<Sample>("SELECT Id, PlasmaPressure * 2 AS M FROM shipslog", s => TimeSpan.Zero, rewrite);

            // Assert
            CollectionAssert.AreEquivalent(expected, seen);
        }

        [TestMethod]
        public void DefaultColumnNamesAreTheirText()
        {
            // Act
            var row = Run("SELECT average(DriveSpin), count(*) FROM shipslog").Single();

            // Assert
            Assert.AreEqual(3975.0, row["average(DriveSpin)"]);
            Assert.AreEqual(8, row["count(*)"]);
        }

        [TestMethod]
        public void AvgAndAverageAreTheSame()
        {
            // Act
            var row = Run("SELECT avg(DriveSpin) AS A, average(DriveSpin) AS B FROM shipslog").Single();

            // Assert
            Assert.AreEqual(row["A"], row["B"]);
        }

        [TestMethod]
        public void StddevOfOneValueIsNull()
        {
            // Act
            var stddev = Run("SELECT stddev(DriveSpin) AS S FROM shipslog WHERE Id = 1").Single()["S"];

            // Assert
            Assert.IsNull(stddev);
        }

        [TestMethod]
        [DataRow(0, 850.0)]
        [DataRow(50, 3950.0)]
        [DataRow(95, 7305.0)]
        [DataRow(100, 7900.0)]
        public void PercentileInterpolatesBetweenRanks(int p, double expected)
        {
            // Act
            var percentile = (double)Run("SELECT percentile(DriveSpin, " + p.ToString(CultureInfo.InvariantCulture) + ") AS P FROM shipslog").Single()["P"];

            // Assert
            // sorted DriveSpin: 850 850 2500 3100 4800 5600 6200 7900
            Assert.AreEqual(expected, percentile, 1e-9);
        }

        [TestMethod]
        public void Median()
        {
            // Act
            var median = Run("SELECT median(DriveSpin) AS M FROM shipslog").Single()["M"];

            // Assert
            Assert.AreEqual(3950.0, median);
        }

        [TestMethod]
        public void SeveralPercentilesGiveAColumnEach()
        {
            // Act
            var row = Run("SELECT percentile(DriveSpin, 0, 100) FROM shipslog").Single();
            var aliased = Run("SELECT percentile(DriveSpin, 0, 100) AS P FROM shipslog").Single();

            // Assert
            Assert.AreEqual(850.0, row["percentile(DriveSpin, 0)"]);
            Assert.AreEqual(7900.0, row["percentile(DriveSpin, 100)"]);
            Assert.AreEqual(850.0, aliased["P.0"]);
            Assert.AreEqual(7900.0, aliased["P.100"]);
        }

        [TestMethod]
        public void LatestAndEarliestSkipNulls()
        {
            // Act
            var row = Run("SELECT latest(Note) AS LastNote, earliest(DriveSpin) AS FirstDriveSpin, latest(FuelMix) AS LastFuelMix FROM shipslog WHERE Id < 7").Single();

            // Assert
            Assert.AreEqual("Kaylon", row["LastNote"]);
            Assert.AreEqual(850, row["FirstDriveSpin"]);
            Assert.AreEqual(11.8, row["LastFuelMix"]);
        }

        [TestMethod]
        public void RatePerBucket()
        {
            // Act
            var rows = Run("SELECT rate(count(*), 1 second) AS R FROM shipslog TIMESERIES 1 second");

            // Assert
            CollectionAssert.AreEqual(new object[] { 2.0, 2.0, 2.0, 2.0 }, Column(rows, "R"));
        }

        [TestMethod]
        public void RateOverTheWindow()
        {
            // Act
            var countPerMinute = Run("SELECT rate(count(*), 1 minute) AS R FROM shipslog SINCE 0 UNTIL 4").Single()["R"];
            var driveStagePerSecond = Run("SELECT rate(sum(DriveStage), 1 second) AS R FROM shipslog SINCE 0 UNTIL 4").Single()["R"];

            // Assert
            Assert.AreEqual(120.0, countPerMinute);
            Assert.AreEqual(3.75, driveStagePerSecond);
        }

        [TestMethod]
        public void MathInsideAggregates()
        {
            // Act
            var row = Run("SELECT average(DriveSpin / 1000.0) AS K, max(PlasmaPressure * 2) AS M FROM shipslog").Single();

            // Assert
            Assert.AreEqual(3.975, (double)row["K"], 1e-12);
            Assert.AreEqual(202.6, row["M"]);
        }

        [TestMethod]
        public void AggregateArithmeticUsesOnlyPairedSamples()
        {
            // Arrange
            var source = new[] { Row(1d, .5), Row(1.5, 1d), Row(100d, null), Row(null, 200d) };
            var query = new TimeSeriesQuery<IDictionary<string, object>>(
                "SELECT count(Measured - Ordered) AS N, average(Measured - Ordered) AS Mean, percentile(Measured - Ordered, 90) AS P90 FROM shipslog",
                _ => TimeSpan.Zero);

            // Act
            var result = query.Evaluate(source).Single();

            // Assert
            Assert.AreEqual(2, result["N"]);
            Assert.AreEqual(.5, result["Mean"]);
            Assert.AreEqual(.5, result["P90"]);
        }

        [TestMethod]
        public void MathBetweenAggregates()
        {
            // Act
            var row = Run("SELECT max(DriveSpin) - min(DriveSpin) AS Spread, (max(PlasmaPressure) + min(PlasmaPressure)) / 2 AS Mid FROM shipslog").Single();

            // Assert
            Assert.AreEqual(7050.0, row["Spread"]);
            Assert.AreEqual((101.3 + 29.9) / 2, (double)row["Mid"], 1e-12);
        }

        [TestMethod]
        public void MathBetweenAggregatesIsNullWhenAnAggregateIs()
        {
            // Act
            var rows = Run("SELECT max(PlasmaPressure) - min(PlasmaPressure) AS D FROM shipslog WHERE Quantum = TRUE TIMESERIES 1 second");

            // Assert
            CollectionAssert.AreEqual(new object[] { null, 0.0, 0.0, 0.0 }, Column(rows, "D"));
        }

        [TestMethod]
        public void CompareWith()
        {
            // Act
            var rows = Run("SELECT count(*) AS N, max(DriveSpin) AS TopSpin FROM shipslog SINCE 2 UNTIL 4 COMPARE WITH 2 seconds ago");

            // Assert
            Assert.HasCount(2, rows);
            Assert.AreEqual("current", rows[0]["Comparison"]);
            Assert.AreEqual(7900, rows[0]["TopSpin"]);
            Assert.AreEqual("previous", rows[1]["Comparison"]);
            Assert.AreEqual(6200, rows[1]["TopSpin"]);
        }

        [TestMethod]
        public void CompareWithTimeseries()
        {
            // Act
            var rows = Run("SELECT max(DriveSpin) AS TopSpin FROM shipslog SINCE 2 UNTIL 4 TIMESERIES 1 second COMPARE WITH 2 seconds");

            // Assert
            CollectionAssert.AreEqual(new object[] { "current", "current", "previous", "previous" }, Column(rows, "Comparison"));
            CollectionAssert.AreEqual(new object[] { TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3), TimeSpan.Zero, TimeSpan.FromSeconds(1) }, Column(rows, "BeginTime"));
            CollectionAssert.AreEqual(new object[] { 7900, 5600, 2500, 6200 }, Column(rows, "TopSpin"));
        }

        [TestMethod]
        public void FacetsRankByTheFirstAggregate()
        {
            // Act
            var rows = Run("SELECT max(DriveSpin) AS TopSpin FROM shipslog FACET DriveStage");

            // Assert
            CollectionAssert.AreEqual(new object[] { 3, 2, 4, 1, 0 }, Column(rows, "DriveStage"));
        }

        [TestMethod]
        public void LimitKeepsTopFacetsAcrossTheTimeseries()
        {
            // Act
            var rows = Run("SELECT max(DriveSpin) AS TopSpin FROM shipslog FACET DriveStage LIMIT 2 TIMESERIES 2 seconds");

            // Assert
            CollectionAssert.AreEqual(new object[] { 3, 2, 3, 2 }, Column(rows, "DriveStage"));
            CollectionAssert.AreEqual(new object[] { null, 6200, 7900, null }, Column(rows, "TopSpin"));
        }

        [TestMethod]
        public void FacetOnAnExpression()
        {
            // Act
            var rows = Run("SELECT count(*) AS N FROM shipslog FACET DriveSpin > 5000");

            // Assert
            Assert.AreEqual(3, rows.Single(r => (bool)r["DriveSpin > 5000"])["N"]);
        }

        [TestMethod]
        public void SelectWithoutAggregatesReturnsRowsInTimeOrder()
        {
            // Arrange
            var shuffled = GetSamples().OrderByDescending(s => s.Id).ToList();
            var query = new TimeSeriesQuery<Sample>("SELECT Id, PlasmaPressure * 2 AS DoublePlasmaPressure FROM shipslog WHERE Quantum = TRUE", s => TimeSpan.FromSeconds(s.Time));

            // Act
            var rows = query.Evaluate(shuffled);

            // Assert
            CollectionAssert.AreEqual(new object[] { 4, 5, 7 }, Column(rows, "Id"));
            CollectionAssert.AreEqual(new object[] { 197.4, 202.6, 184.0 }, Column(rows, "DoublePlasmaPressure"));
            CollectionAssert.AreEqual(new object[] { TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3) }, Column(rows, "Time"));
        }

        [TestMethod]
        public void LimitWithoutAggregatesKeepsTheFirstRows()
        {
            // Act
            var rows = Run("SELECT Id FROM shipslog LIMIT 2");

            // Assert
            CollectionAssert.AreEqual(new object[] { 1, 2 }, Column(rows, "Id"));
        }

        [TestMethod]
        public void BucketsHandleBoundariesOutliersMissingValuesAndCulture()
        {
            // Arrange
            var culture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            try
            {
                var bucket = FacetExpression.Compile<IDictionary<string, object>>("buckets(Measured, 3, 15)");
                var oneBucket = FacetExpression.Compile<IDictionary<string, object>>("buckets(Measured, 10, 1)");
                var tiny = FacetExpression.Compile<IDictionary<string, object>>("buckets(Measured, 1e-27, 1000)");
                var huge = FacetExpression.Compile<IDictionary<string, object>>("buckets(Measured, 1e308, 10)");

                // Act
                var negative = bucket(Row(-1d, null));
                var zero = bucket(Row(0d, null));
                var firstBoundary = bucket(Row(.2, null));
                var innerBoundary = bucket(Row(.6, null));
                var belowInnerBoundary = bucket(Row(Math.BitDecrement(.6), null));
                var lastBoundary = bucket(Row(2.8, null));
                var outlier = bucket(Row(30d, null));
                var missing = bucket(Row(null, null));
                var notANumber = bucket(Row(double.NaN, null));
                var infinity = bucket(Row(double.PositiveInfinity, null));
                var allValues = oneBucket(Row(100, null));
                var tinyZero = tiny(Row(0d, null));
                var tinyWidth = tiny(Row(1e-27, null));
                var hugeZero = huge(Row(0d, null));
                var hugeCeiling = huge(Row(1e308, null));

                // Assert
                Assert.AreEqual("< 0.2", negative);
                Assert.AreEqual("< 0.2", zero);
                Assert.AreEqual("0.2 - < 0.4", firstBoundary);
                Assert.AreEqual("0.6 - < 0.8", innerBoundary);
                Assert.AreEqual("0.4 - < 0.6", belowInnerBoundary);
                Assert.AreEqual(">= 2.8", lastBoundary);
                Assert.AreEqual(">= 2.8", outlier);
                Assert.IsNull(missing);
                Assert.IsNull(notANumber);
                Assert.IsNull(infinity);
                Assert.AreEqual("All values", allValues);
                Assert.AreNotEqual(tinyZero, tinyWidth);
                Assert.AreNotEqual(hugeZero, hugeCeiling);

                // Act & Assert
                Assert.ThrowsExactly<SqlException>(() => bucket(Row("0.6", null)));
            }
            finally
            {
                CultureInfo.CurrentCulture = culture;
            }
        }

        [TestMethod]
        public void MultipleBucketsGroupWithTimeseriesAndCompileTheRewrittenValue()
        {
            // Arrange
            var source = new[] { Row(1000d, .2), Row(1200d, .3), Row(1600d, .7) };
            var query = new TimeSeriesQuery<IDictionary<string, object>>(
                "SELECT count(*) AS N FROM shipslog FACET buckets(Measured, 8000, 16), buckets(Ordered, 2, 4) TIMESERIES 1 second",
                row => TimeSpan.FromSeconds((double)row["Measured"] == 1600 ? 1 : 0));
            var rewritten = FacetExpression.Compile<IDictionary<string, object>>("buckets(Converted, 4, 4)", text => text.Replace("Converted", "Measured - Ordered", StringComparison.Ordinal));

            // Act
            var result = query.Evaluate(source);
            var rewrittenBucket = rewritten(Row(2d, .5));

            // Assert
            Assert.HasCount(4, result);
            Assert.AreEqual(3, result.Sum(row => (int)row["N"]));
            Assert.AreEqual(2, result.Single(row => (string)row["buckets(Measured, 8000, 16)"] == "1000 - < 1500" && (TimeSpan)row["BeginTime"] == TimeSpan.Zero)["N"]);
            Assert.AreEqual("1 - < 2", rewrittenBucket);
        }

        [TestMethod]
        public void CasesUseOrderedPredicatesAndExposeAxisLabelsAndAliases()
        {
            // Arrange
            const string expression = "cases(Measured < 1500 AS '1000', Measured >= 1500 AND Measured < 2500 AS '2000', Measured >= 2500 AS '3000') AS [Drive axis]";
            var facet = FacetExpression.Compile<IDictionary<string, object>>(expression);
            var query = new TimeSeriesQuery<IDictionary<string, object>>("SELECT count(*) AS N FROM shipslog FACET " + expression, _ => TimeSpan.Zero);
            var source = new[] { Row(1000d, null), Row(2000d, null) };
            var overlapping = FacetExpression.Compile<IDictionary<string, object>>("cases(Measured > 0 AS 'Mercer', Measured > 1 AS 'Grayson')");
            var expectedLabels = new[] { "1000", "2000", "3000" };

            // Act
            var negative = facet(Row(-1d, null));
            var belowBoundary = facet(Row(Math.BitDecrement(1500d), null));
            var lowerBoundary = facet(Row(1500d, null));
            var upperBoundary = facet(Row(2500d, null));
            var missing = facet(Row(null, null));
            var name = FacetExpression.Name(expression);
            var labels = FacetExpression.Labels(expression).ToArray();
            var rows = query.Evaluate(source);
            var firstMatch = overlapping(Row(2d, null));

            // Assert
            Assert.AreEqual("1000", negative);
            Assert.AreEqual("1000", belowBoundary);
            Assert.AreEqual("2000", lowerBoundary);
            Assert.AreEqual("3000", upperBoundary);
            Assert.IsNull(missing);
            Assert.AreEqual("Drive axis", name);
            CollectionAssert.AreEqual(expectedLabels, labels);
            Assert.AreEqual(2, rows.Sum(row => (int)row["N"]));
            Assert.IsTrue(rows.All(row => row.ContainsKey("Drive axis")));
            Assert.AreEqual("Mercer", firstMatch);

            // Act & Assert
            Assert.ThrowsExactly<SqlException>(() => FacetExpression.Compile<IDictionary<string, object>>("cases(Measured > 0)"));
        }

        [TestMethod]
        public void LabelsNeedNoCompiling()
        {
            // Arrange
            var expectedCases = new[] { "Mercer's", "Grayson" };

            // Act
            var plasmaAxis = FacetExpression.Labels("buckets(convert(PlasmaPressure, 'kPa'), 3, 15) AS [Plasma axis]");
            var cases = FacetExpression.Labels("cases(DriveStage = 1 AS 'Mercer''s', DriveStage = 2 AS 'Grayson', DriveStage = 3 AS 'Mercer''s')").ToArray();
            var fieldLabels = FacetExpression.Labels("[DriveStage]");
            var fieldName = FacetExpression.Name("[DriveStage]");

            // Assert
            Assert.HasCount(15, plasmaAxis);
            Assert.AreEqual("< 0.2", plasmaAxis[0]);
            Assert.AreEqual("0.2 - < 0.4", plasmaAxis[1]);
            Assert.AreEqual(">= 2.8", plasmaAxis[14]);
            CollectionAssert.AreEqual(expectedCases, cases);
            Assert.IsNull(fieldLabels);
            Assert.AreEqual("DriveStage", fieldName);
        }

        [TestMethod]
        [DataRow("buckets(Measured, 0, 10)")]
        [DataRow("buckets(Measured, -1, 10)")]
        [DataRow("buckets(Measured, NaN, 10)")]
        [DataRow("buckets(Measured, 1e309, 10)")]
        [DataRow("buckets(Measured, 10, 0)")]
        [DataRow("buckets(Measured, 10, 1.5)")]
        [DataRow("buckets(Measured, 10, 1001)")]
        [DataRow("buckets(Measured, 10)")]
        [DataRow("buckets(Measured, 10, 2) + 1")]
        public void InvalidBucketDefinitionsFailBeforeEvaluation(string expression)
        {
            // Act & Assert
            Assert.ThrowsExactly<SqlException>(() => FacetExpression.Compile<IDictionary<string, object>>(expression));
        }
    }
}
