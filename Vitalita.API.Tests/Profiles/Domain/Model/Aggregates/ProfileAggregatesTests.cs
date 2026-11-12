using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Model.Exceptions;

namespace Vitalita.API.Tests.Profiles.Domain.Model.Aggregates;

public class ProfileAggregatesTests
{
    [Fact]
    public void CaregiverProfile_TrimsTextAndCleansCertifications()
    {
        var profile = new CaregiverProfile(1, " Andrea ", " Mendoza ", null, " Lic. ", null, null,
            [" Primeros auxilios ", "", null, "primeros AUXILIOS", "Cuidado geriátrico"]);

        Assert.Equal("Andrea", profile.FirstName);
        Assert.Equal("Mendoza", profile.LastName);
        Assert.Equal(string.Empty, profile.PhoneNumber);
        Assert.Equal("Lic.", profile.ProfessionalTitle);
        Assert.Equal(["Primeros auxilios", "Cuidado geriátrico"], profile.Certifications);
    }

    [Fact]
    public void CaregiverProfile_WithBlankName_Throws()
    {
        Assert.Throws<ArgumentException>(() => new CaregiverProfile(1, " ", "Mendoza", null, null, null, null, []));
    }

    [Fact]
    public void CaregiverProfile_UpdateWithAnotherUser_Throws()
    {
        var profile = new CaregiverProfile(1, "Andrea", "Mendoza", null, null, null, null, []);

        Assert.Throws<OwnerChangeNotAllowedException>(() =>
            profile.Update(2, "Andrea", "Mendoza", null, null, null, null, []));
    }

    [Fact]
    public void FamilyMemberProfile_UpdateWithSameUser_ReplacesData()
    {
        var profile = new FamilyMemberProfile(2, "Renzo", "Rodríguez", "987111222");

        profile.Update(2, "Renzo A.", "Rodríguez", "");

        Assert.Equal("Renzo A.", profile.FirstName);
        Assert.Equal(string.Empty, profile.PhoneNumber);
        Assert.Throws<OwnerChangeNotAllowedException>(() => profile.Update(3, "X", "Y", null));
    }

    [Fact]
    public void Profiles_WithNonPositiveUser_Throw()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FamilyMemberProfile(0, "Renzo", "R", null));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CaregiverProfile(-1, "Andrea", "M", null, null, null, null, []));
    }
}
