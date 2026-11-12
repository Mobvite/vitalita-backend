using Vitalita.API.Profiles.Domain.Model.Exceptions;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Domain.Model;

namespace Vitalita.API.Profiles.Domain.Model.Aggregates;

/// <summary>
///     Family member profile aggregate root.
/// </summary>
/// <remarks>One profile per IAM user (unique <see cref="UserId" />); the owner never changes.</remarks>
public class FamilyMemberProfile : IAuditableEntity
{
    /// <summary>
    ///     Protected parameterless constructor for EF Core.
    /// </summary>
    protected FamilyMemberProfile()
    {
        FirstName = LastName = PhoneNumber = string.Empty;
    }

    /// <summary>
    ///     Creates the profile of a family member.
    /// </summary>
    /// <param name="userId">IAM user identifier.</param>
    /// <param name="firstName">First name (required).</param>
    /// <param name="lastName">Last name (required).</param>
    /// <param name="phoneNumber">Phone number.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="userId" /> is not positive.</exception>
    /// <exception cref="ArgumentException">Thrown when a name is empty.</exception>
    public FamilyMemberProfile(int userId, string firstName, string lastName, string? phoneNumber) : this()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId);
        UserId = userId;
        SetData(firstName, lastName, phoneNumber);
    }

    /// <summary>Gets the server-generated identifier.</summary>
    public int Id { get; private set; }

    /// <summary>Gets the IAM user that owns the profile.</summary>
    public int UserId { get; private set; }

    /// <summary>Gets the first name.</summary>
    public string FirstName { get; private set; }

    /// <summary>Gets the last name.</summary>
    public string LastName { get; private set; }

    /// <summary>Gets the phone number (may be empty).</summary>
    public string PhoneNumber { get; private set; }

    /// <inheritdoc />
    public DateTime? CreatedAt { get; set; }

    /// <inheritdoc />
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    ///     Replaces the profile data.
    /// </summary>
    /// <param name="userId">The owner sent by the client; it must be the current owner.</param>
    /// <param name="firstName">First name (required).</param>
    /// <param name="lastName">Last name (required).</param>
    /// <param name="phoneNumber">Phone number.</param>
    /// <exception cref="OwnerChangeNotAllowedException">Thrown when <paramref name="userId" /> is another user.</exception>
    public void Update(int userId, string firstName, string lastName, string? phoneNumber)
    {
        if (userId != UserId) throw new OwnerChangeNotAllowedException();
        SetData(firstName, lastName, phoneNumber);
    }

    private void SetData(string firstName, string lastName, string? phoneNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        FirstName = TextNormalizer.Clean(firstName);
        LastName = TextNormalizer.Clean(lastName);
        PhoneNumber = TextNormalizer.Clean(phoneNumber);
    }
}
