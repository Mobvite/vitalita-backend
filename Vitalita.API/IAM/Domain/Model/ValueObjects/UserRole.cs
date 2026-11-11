namespace Vitalita.API.IAM.Domain.Model.ValueObjects;

/// <summary>
///     Roles of a Vitalita account. Serialized as strings (<c>"Caregiver"</c>, <c>"FamilyMember"</c>, <c>"Admin"</c>).
/// </summary>
public enum UserRole
{
    /// <summary>
    ///     Nurse or caregiver who takes care of older adults.
    /// </summary>
    Caregiver,

    /// <summary>
    ///     Family member who follows the care of an older adult.
    /// </summary>
    FamilyMember,

    /// <summary>
    ///     Platform administrator. It can never be obtained through the public sign-up.
    /// </summary>
    Admin
}
