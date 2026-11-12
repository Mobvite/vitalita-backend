using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Model.Exceptions;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Tests.Profiles.Fakes;

namespace Vitalita.API.Tests.Profiles.Domain.Model.Aggregates;

public class OlderAdultTests
{
    private static readonly DateOnly Today = new(2026, 11, 11);

    [Fact]
    public void Constructor_NormalizesTextAndLists()
    {
        var olderAdult = new OlderAdult(3, OlderAdultTestData.Valid(bloodType: "ab+"), Today);

        Assert.Equal(3, olderAdult.CaregiverId);
        Assert.Equal("María", olderAdult.FirstName);
        Assert.Equal("AB+", olderAdult.BloodType);
        Assert.Equal(["Penicilina"], olderAdult.Allergies);
        Assert.Equal("Renzo Rodríguez", olderAdult.EmergencyContact.Name);
        Assert.True(olderAdult.EmergencyContact.IsComplete);
        Assert.True(olderAdult.IsInChargeOf(3));
        Assert.False(olderAdult.IsInChargeOf(4));
    }

    [Fact]
    public void Constructor_WithBirthDateToday_IsAccepted()
    {
        var olderAdult = new OlderAdult(3, OlderAdultTestData.Valid(Today), Today);

        Assert.Equal(Today, olderAdult.BirthDate);
    }

    [Fact]
    public void Constructor_WithBirthDateInTheFuture_Throws()
    {
        Assert.Throws<BirthDateInFutureException>(() =>
            new OlderAdult(3, OlderAdultTestData.Valid(Today.AddDays(1)), Today));
    }

    [Fact]
    public void Constructor_WithInvalidBloodType_Throws()
    {
        Assert.Throws<ArgumentException>(() => new OlderAdult(3, OlderAdultTestData.Valid(bloodType: "Z+"), Today));
    }

    [Fact]
    public void Update_WithAnotherCaregiver_Throws()
    {
        var olderAdult = new OlderAdult(3, OlderAdultTestData.Valid(), Today);

        Assert.Throws<OwnerChangeNotAllowedException>(() =>
            olderAdult.Update(4, OlderAdultTestData.Valid(), Today));
    }

    [Fact]
    public void Update_ChangesStatus()
    {
        var olderAdult = new OlderAdult(3, OlderAdultTestData.Valid(), Today);

        olderAdult.Update(3, OlderAdultTestData.Valid(status: OlderAdultStatus.Archived), Today);

        Assert.Equal(OlderAdultStatus.Archived, olderAdult.Status);
    }
}
