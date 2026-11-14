using Vitalita.API.Profiles.Domain.Model.Commands;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Domain.Model.ValueObjects;

namespace Vitalita.API.Tests.Profiles.Domain.Model.ValueObjects;

public class ProfilesValueObjectsTests
{
    [Theory]
    [InlineData(" vita-7kq2m9xh ", "VITA-7KQ2M9XH")]
    [InlineData("VITA-8H2K", "VITA-8H2K")]
    public void InvitationCode_IsNormalizedToUppercase(string raw, string expected)
    {
        Assert.Equal(expected, new InvitationCode(raw).Value);
        Assert.Equal(new InvitationCode(expected), new InvitationCode(raw));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("VITA-1234567890123456")]
    public void InvitationCode_RejectsEmptyOrTooLongValues(string? raw)
    {
        Assert.False(InvitationCode.IsValid(raw));
        Assert.Throws<ArgumentException>(() => new InvitationCode(raw!));
    }

    [Theory]
    [InlineData("VITA-7KQ2M9XH", true)]
    [InlineData("VITA-8H2K", false)]
    [InlineData("VITA-7KQ2M9X0", false)]
    [InlineData("XXXX-7KQ2M9XH", false)]
    public void InvitationCode_HasGeneratedFormat(string code, bool expected)
    {
        Assert.Equal(expected, InvitationCode.HasGeneratedFormat(code));
    }

    [Theory]
    [InlineData("", true)]
    [InlineData(null, true)]
    [InlineData("O+", true)]
    [InlineData(" ab- ", true)]
    [InlineData("C+", false)]
    public void BloodTypes_IsValid(string? value, bool expected)
    {
        Assert.Equal(expected, BloodTypes.IsValid(value));
    }

    [Fact]
    public void EmergencyContact_Create_TrimsValues()
    {
        var contact = EmergencyContact.Create(" Renzo ", null, null);

        Assert.Equal("Renzo", contact.Name);
        Assert.Equal(string.Empty, contact.PhoneNumber);
        Assert.False(contact.IsComplete);
        Assert.False(EmergencyContact.Empty.IsComplete);
    }

    [Fact]
    public void Requester_RoleHelpers()
    {
        Assert.True(new Requester(1, "Admin", null).IsAdmin);
        Assert.True(new Requester(1, "Caregiver", null).IsCaregiver);
        Assert.True(new Requester(1, "FamilyMember", null).IsFamilyMember);
        Assert.False(new Requester(1, null, null).IsAdmin);
    }

    [Fact]
    public void RequesterAndInvitationCommand_DoNotPrintEmails()
    {
        var requester = new Requester(1, "Caregiver", "andrea@vitalita.test");
        var command = new CreateFamilyAccessCommand(requester, 1, "carla@correo.pe", FamilyRelationship.Daughter);

        Assert.DoesNotContain("andrea@vitalita.test", requester.ToString());
        Assert.DoesNotContain("carla@correo.pe", command.ToString());
        Assert.Contains("OlderAdultId = 1", command.ToString());
    }
}
