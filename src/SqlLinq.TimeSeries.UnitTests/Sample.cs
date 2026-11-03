namespace SqlLinq.TimeSeries.UnitTests
{
    sealed class Sample
    {
        public int Id { get; set; }
        public double Time { get; set; }
        public int DriveSpin { get; set; }
        public double PlasmaPressure { get; set; }
        public double? FuelMix { get; set; }
        public int DriveStage { get; set; }
        public bool Quantum { get; set; }
        public string Note { get; set; }
    }
}
