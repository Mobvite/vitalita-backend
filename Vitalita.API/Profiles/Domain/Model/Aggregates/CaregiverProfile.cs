using Vitalita.API.Profiles.Domain.Model.Exceptions;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Domain.Model;

namespace Vitalita.API.Profiles.Domain.Model.Aggregates;

/// <summary>
///     Caregiver profile aggregate root. It holds the professional data that family members can see.
/// </summary>
/// <remarks>
///     One profile per IAM user (unique <see cref="UserId" />); the owner never changes. Text is trimmed and empty
///     certifications are removed.
/// </remarks>
public class CaregiverProfile : IAuditableEntity
{
    /// <summary>
    ///     Protected parameterless constructor for EF Core.
    /// </summary>
    protected CaregiverProfile()
    {
        FirstName = LastName = PhoneNumber = ProfessionalTitle = Bio = PhotoUrl = string.Empty;
        Certifications = [];
    }

    /// <summary>
    ///     Creates the profile of a caregiver.
    /// </summary>
    /// <param name="userId">IAM user identifier.</param>
    /// <param name="firstName">First name (required).</param>
    /// <param name="lastName">Last name (required).</param>
    /// <param name="phoneNumber">Phone number.</param>
    /// <param name="professionalTitle">Professional title.</param>
    /// <param name="bio">Short presentation.</param>
    /// <param name="photoUrl">Photo URL.</param>
    /// <param name="certifications">Certifications and courses.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="userId" /> is not positive.</exception>
    /// <exception cref="ArgumentException">Thrown when a name is empty.</exception>
    public CaregiverProfile(int userId, string firstName, string lastName, string? phoneNumber,
        string? professionalTitle, string? bio, string? photoUrl, IEnumerable<string?>? certifications) : this()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId);
        UserId = userId;
        SetData(firstName, lastName, phoneNumber, professionalTitle, bio, photoUrl, certifications);
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

    /// <summary>Gets the professional title (may be empty).</summary>
    public string ProfessionalTitle { get; private set; }

    /// <summary>Gets the short presentation (may be empty).</summary>
    public string Bio { get; private set; }

    /// <summary>Gets the photo URL (may be empty).</summary>
    public string PhotoUrl { get; private set; }

    /// <summary>Gets the certifications and courses.</summary>
    public IReadOnlyList<string> Certifications { get; private set; }

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
    /// <param name="professionalTitle">Professional title.</param>
    /// <param name="bio">Short presentation.</param>
    /// <param name="photoUrl">Photo URL.</param>
    /// <param name="certifications">Certifications and courses.</param>
    /// <exception cref="OwnerChangeNotAllowedException">Thrown when <paramref name="userId" /> is another user.</exception>
    public void Update(int userId, string firstName, string lastName, string? phoneNumber, string? professionalTitle,
        string? bio, string? photoUrl, IEnumerable<string?>? certifications)
    {
        if (userId != UserId) throw new OwnerChangeNotAllowedException();
        SetData(firstName, lastName, phoneNumber, professionalTitle, bio, photoUrl, certifications);
    }

    private void SetData(string firstName, string lastName, string? phoneNumber, string? professionalTitle,
        string? bio, string? photoUrl, IEnumerable<string?>? certifications)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        FirstName = TextNormalizer.Clean(firstName);
        LastName = TextNormalizer.Clean(lastName);
        PhoneNumber = TextNormalizer.Clean(phoneNumber);
        ProfessionalTitle = TextNormalizer.Clean(professionalTitle);
        Bio = TextNormalizer.Clean(bio);
        PhotoUrl = TextNormalizer.Clean(photoUrl);
        Certifications = TextNormalizer.CleanList(certifications);
    }
}
