using Vitalita.API.IAM.Domain.Model.ValueObjects;

namespace Vitalita.API.Tests.IAM.Domain.Model.ValueObjects;

public class EmailAddressTests
{
    [Fact]
    public void Constructor_TrimsAndLowercases()
    {
        var email = new EmailAddress("  Andrea.Mendoza@Vitalita.PE ");

        Assert.Equal("andrea.mendoza@vitalita.pe", email.Value);
    }

    [Fact]
    public void Equality_IgnoresCase()
    {
        Assert.Equal(new EmailAddress("A@vitalita.pe"), new EmailAddress("a@VITALITA.pe"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("andrea")]
    [InlineData("andrea@vitalita")]
    [InlineData("andrea@vitalita.p")]
    [InlineData("an drea@vitalita.pe")]
    public void Constructor_RejectsInvalidValues(string value)
    {
        Assert.Throws<ArgumentException>(() => new EmailAddress(value));
        Assert.False(EmailAddress.IsValid(value));
    }

    [Fact]
    public void Constructor_RejectsValuesLongerThan254Characters()
    {
        var value = new string('a', 250) + "@a.pe";

        Assert.Throws<ArgumentException>(() => new EmailAddress(value));
    }
}
