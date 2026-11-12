using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Monitoring.Domain.Model.Exceptions;
using Vitalita.API.Monitoring.Domain.Model.ValueObjects;

namespace Vitalita.API.Tests.Monitoring.Domain.Model.Aggregates;

public class VitalSignTests
{
    private static readonly DateTime Now = new(2026, 11, 11, 15, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(VitalSignType.HeartRate, 20, null)]
    [InlineData(VitalSignType.HeartRate, 250, null)]
    [InlineData(VitalSignType.OxygenSaturation, 100, null)]
    [InlineData(VitalSignType.Temperature, 36.6, null)]
    [InlineData(VitalSignType.Glucose, 600, null)]
    [InlineData(VitalSignType.BloodPressure, 120, 80.0)]
    [InlineData(VitalSignType.BloodPressure, 260, 160.0)]
    public void Check_ValidMeasurements(VitalSignType type, double value, double? secondary)
    {
        Assert.Null(VitalSign.Check(type, (decimal)value, (decimal?)secondary));
    }

    [Theory]
    [InlineData(VitalSignType.HeartRate, 19, null, VitalSignValidationError.ValueOutOfRange)]
    [InlineData(VitalSignType.HeartRate, 251, null, VitalSignValidationError.ValueOutOfRange)]
    [InlineData(VitalSignType.Temperature, 45.1, null, VitalSignValidationError.ValueOutOfRange)]
    [InlineData(VitalSignType.Glucose, 110, 5.0, VitalSignValidationError.SecondaryValueNotAllowed)]
    [InlineData(VitalSignType.BloodPressure, 120, null, VitalSignValidationError.SecondaryValueRequired)]
    [InlineData(VitalSignType.BloodPressure, 120, 29.0, VitalSignValidationError.SecondaryValueOutOfRange)]
    [InlineData(VitalSignType.BloodPressure, 120, 161.0, VitalSignValidationError.SecondaryValueOutOfRange)]
    [InlineData(VitalSignType.BloodPressure, 80, 80.0, VitalSignValidationError.SystolicNotGreaterThanDiastolic)]
    public void Check_InvalidMeasurements(VitalSignType type, double value, double? secondary,
        VitalSignValidationError expected)
    {
        Assert.Equal(expected, VitalSign.Check(type, (decimal)value, (decimal?)secondary));
        var exception = Assert.Throws<InvalidVitalSignException>(() =>
            new VitalSign(1, type, (decimal)value, (decimal?)secondary, Now, Now));
        Assert.Equal(expected, exception.Error);
    }

    [Theory]
    [InlineData(VitalSignType.HeartRate, "bpm")]
    [InlineData(VitalSignType.BloodPressure, "mmHg")]
    [InlineData(VitalSignType.OxygenSaturation, "%")]
    [InlineData(VitalSignType.Temperature, "°C")]
    [InlineData(VitalSignType.Glucose, "mg/dL")]
    public void Unit_IsDerivedFromTheType(VitalSignType type, string unit)
    {
        var secondary = type == VitalSignType.BloodPressure ? 80m : (decimal?)null;
        var value = type switch
        {
            VitalSignType.Temperature => 36.6m,
            VitalSignType.OxygenSaturation => 97m,
            _ => 120m
        };

        Assert.Equal(unit, new VitalSign(1, type, value, secondary, Now, Now).Unit);
    }

    [Fact]
    public void MeasuredAt_InTheFuture_Throws()
    {
        Assert.Throws<InstantInFutureException>(() =>
            new VitalSign(1, VitalSignType.HeartRate, 70, null, Now.AddHours(1), Now));
    }

    [Fact]
    public void Update_ChangesTypeAndUnit_ButNotTheOlderAdult()
    {
        var sign = new VitalSign(1, VitalSignType.HeartRate, 70, null, Now, Now);

        sign.Update(1, VitalSignType.BloodPressure, 130, 85, Now, Now);

        Assert.Equal("mmHg", sign.Unit);
        Assert.Equal(85, sign.SecondaryValue);
        Assert.Throws<OlderAdultChangeNotAllowedException>(() =>
            sign.Update(2, VitalSignType.HeartRate, 70, null, Now, Now));
    }
}
