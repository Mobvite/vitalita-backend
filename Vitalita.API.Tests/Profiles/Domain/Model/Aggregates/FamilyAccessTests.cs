using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Model.Exceptions;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;

namespace Vitalita.API.Tests.Profiles.Domain.Model.Aggregates;

public class FamilyAccessTests
{
    private static readonly DateTime Now = new(2026, 11, 11, 15, 0, 0, DateTimeKind.Utc);

    private static FamilyAccess Invitation(string email = " Carla.Rodriguez@Correo.PE ") =>
        new(1, email, FamilyRelationship.Daughter, new InvitationCode("VITA-7KQ2M9XH"), Now);

    [Fact]
    public void Constructor_CreatesAPendingInvitationThatExpiresInSevenDays()
    {
        var access = Invitation();

        Assert.Equal(AccessStatus.Pending, access.Status);
        Assert.Equal("carla.rodriguez@correo.pe", access.InvitedEmail);
        Assert.Null(access.FamilyMemberId);
        Assert.Null(access.AcceptedAt);
        Assert.Equal(Now, access.InvitedAt);
        Assert.Equal(Now.AddDays(7), access.ExpiresAt);
        Assert.Equal(DateTimeKind.Utc, access.ExpiresAt.Kind);
    }

    [Fact]
    public void Accept_ByTheInvitedEmail_ActivatesTheAccess()
    {
        var access = Invitation();
        var later = Now.AddHours(2);

        access.Accept(5, "CARLA.rodriguez@correo.pe", later);

        Assert.Equal(AccessStatus.Active, access.Status);
        Assert.Equal(5, access.FamilyMemberId);
        Assert.Equal(later, access.AcceptedAt);
        Assert.True(access.IsActive);
    }

    [Fact]
    public void Accept_ByAnotherEmail_Throws()
    {
        var access = Invitation();

        Assert.Throws<InvitationEmailMismatchException>(() => access.Accept(5, "other@correo.pe", Now));
        Assert.Equal(AccessStatus.Pending, access.Status);
    }

    [Fact]
    public void Accept_AtOrAfterExpiration_Throws()
    {
        var access = Invitation();

        Assert.Throws<InvitationNotAcceptableException>(() =>
            access.Accept(5, "carla.rodriguez@correo.pe", access.ExpiresAt));
        Assert.True(access.IsExpired(access.ExpiresAt));
        Assert.False(access.CanBeAccepted(access.ExpiresAt));
        Assert.True(access.CanBeAccepted(access.ExpiresAt.AddTicks(-1)));
    }

    [Fact]
    public void Accept_WhenRevoked_Throws()
    {
        var access = Invitation();
        access.Revoke();

        Assert.Throws<InvitationNotAcceptableException>(() => access.Accept(5, "carla.rodriguez@correo.pe", Now));
    }

    [Fact]
    public void Accept_Twice_Throws()
    {
        var access = Invitation();
        access.Accept(5, "carla.rodriguez@correo.pe", Now);

        Assert.Throws<InvitationNotAcceptableException>(() => access.Accept(6, "carla.rodriguez@correo.pe", Now));
        Assert.Equal(5, access.FamilyMemberId);
    }

    [Fact]
    public void Revoke_ActiveAccess_IsFinal()
    {
        var access = Invitation();
        access.Accept(5, "carla.rodriguez@correo.pe", Now);

        access.Revoke();
        access.Revoke();

        Assert.Equal(AccessStatus.Revoked, access.Status);
        Assert.False(access.IsActive);
    }

    [Fact]
    public void ChangeRelationship_WhenRevoked_Throws()
    {
        var access = Invitation();
        access.Revoke();

        Assert.Throws<InvalidAccessStatusTransitionException>(() => access.ChangeRelationship(FamilyRelationship.Son));
        access.ChangeRelationship(FamilyRelationship.Daughter); // same value: no change, no error
    }

    [Fact]
    public void ChangeRelationship_WhenPending_ChangesIt()
    {
        var access = Invitation();

        access.ChangeRelationship(FamilyRelationship.Grandchild);

        Assert.Equal(FamilyRelationship.Grandchild, access.Relationship);
    }
}
