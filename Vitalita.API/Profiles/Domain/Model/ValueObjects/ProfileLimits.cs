namespace Vitalita.API.Profiles.Domain.Model.ValueObjects;

/// <summary>
///     Lengths, sizes and formats shared by the Profiles aggregates, resources and database columns.
/// </summary>
public static class ProfileLimits
{
    /// <summary>Maximum length of first and last names.</summary>
    public const int NameMaxLength = 100;

    /// <summary>Maximum length of a phone number.</summary>
    public const int PhoneNumberMaxLength = 20;

    /// <summary>Empty, or 6 to 20 characters among digits, spaces and an optional leading <c>+</c>.</summary>
    public const string PhoneNumberPattern = @"^$|^\+?[0-9 ]{6,20}$";

    /// <summary>Maximum length of the professional title.</summary>
    public const int ProfessionalTitleMaxLength = 100;

    /// <summary>Maximum length of the caregiver presentation.</summary>
    public const int BioMaxLength = 500;

    /// <summary>Maximum length of the photo URL.</summary>
    public const int PhotoUrlMaxLength = 500;

    /// <summary>Empty, or an absolute <c>http</c>/<c>https</c> URL.</summary>
    public const string PhotoUrlPattern = @"^$|^https?://\S+$";

    /// <summary>Maximum length of a single item of a list (certification, allergy, condition).</summary>
    public const int ListItemMaxLength = 100;

    /// <summary>Maximum number of certifications of a caregiver.</summary>
    public const int CertificationsMaxCount = 20;

    /// <summary>Maximum number of allergies or chronic conditions of an older adult.</summary>
    public const int ClinicalListMaxCount = 30;

    /// <summary>Length of a Peruvian national ID (DNI).</summary>
    public const int DocumentNumberLength = 8;

    /// <summary>Empty, or exactly 8 digits (same rule as the frontend form).</summary>
    public const string DocumentNumberPattern = @"^$|^\d{8}$";

    /// <summary>Maximum length of the blood type column.</summary>
    public const int BloodTypeMaxLength = 3;

    /// <summary>Maximum length of the care observations.</summary>
    public const int ObservationsMaxLength = 1000;

    /// <summary>Maximum length of an email (RFC 5321).</summary>
    public const int EmailMaxLength = 254;

    /// <summary>Maximum length of the enum columns.</summary>
    public const int EnumMaxLength = 20;
}
