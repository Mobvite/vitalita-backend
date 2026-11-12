namespace Vitalita.API.Profiles.Application.Errors;

/// <summary>
///     Errors that can occur when a family access is updated.
/// </summary>
public enum UpdateFamilyAccessError
{
    /// <summary>
    ///     The family access does not exist.
    /// </summary>
    NotFound,

    /// <summary>
    ///     The requester cannot make this change (not the caregiver in charge, not the invited person, or not their own family member profile).
    /// </summary>
    Forbidden,

    /// <summary>
    ///     The access is accepted without a familyMemberId.
    /// </summary>
    FamilyMemberRequired,

    /// <summary>
    ///     The family member profile does not exist (administrators only).
    /// </summary>
    FamilyMemberNotFound,

    /// <summary>
    ///     The invitation is no longer pending or has expired.
    /// </summary>
    InvitationNotAcceptable,

    /// <summary>
    ///     The status change is not allowed (revoked is final; active cannot go back to pending).
    /// </summary>
    InvalidStatusTransition,

    /// <summary>
    ///     An unexpected error occurred while saving the access.
    /// </summary>
    UnexpectedError
}
