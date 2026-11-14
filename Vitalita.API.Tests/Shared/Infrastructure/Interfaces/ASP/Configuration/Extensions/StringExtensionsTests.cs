using Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Configuration.Extensions;

namespace Vitalita.API.Tests.Shared.Infrastructure.Interfaces.ASP.Configuration.Extensions;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("OlderAdults", "older-adults")]
    [InlineData("FamilyAccess", "family-access")]
    [InlineData("CaregiverProfiles", "caregiver-profiles")]
    [InlineData("MedicationAdministrations", "medication-administrations")]
    [InlineData("Authentication", "authentication")]
    [InlineData("familyMemberProfiles", "family-member-profiles")]
    [InlineData("", "")]
    public void ToKebabCase_ConvertsControllerNamesToRouteSegments(string input, string expected)
    {
        Assert.Equal(expected, input.ToKebabCase());
    }
}
