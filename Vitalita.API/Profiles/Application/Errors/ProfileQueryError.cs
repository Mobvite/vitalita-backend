namespace Vitalita.API.Profiles.Application.Errors;

/// <summary>
///     Errors that can occur when a Profiles query is handled.
/// </summary>
public enum ProfileQueryError
{
    /// <summary>
    ///     The resource does not exist.
    /// </summary>
    NotFound,

    /// <summary>
    ///     The requester cannot read the resource, or the filter points to a resource of another user.
    /// </summary>
    Forbidden
}
