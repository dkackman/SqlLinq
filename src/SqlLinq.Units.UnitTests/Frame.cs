using UnitsNet.Metadata.Annotations;
using UnitsNet.Units;

namespace SqlLinq.Units.UnitTests
{
    sealed class Frame
    {
        public double Time { get; set; }

        [Quantity(TemperatureUnit.DegreeCelsius)]
        public double CoreTemp { get; set; }

        [Quantity(PressureUnit.Kilopascal)]
        [AllowUnitConversion(PressureUnit.PoundForcePerSquareInch)]
        [AllowUnitConversion(PressureUnit.Bar)]
        public double PlasmaPressure { get; set; }

        [Quantity(RotationalSpeedUnit.RevolutionPerMinute)]
        public int DriveSpin { get; set; }

        public double Scan { get; set; }
    }
}
