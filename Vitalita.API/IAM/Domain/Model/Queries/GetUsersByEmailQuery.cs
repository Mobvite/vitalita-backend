using Vitalita.API.IAM.Domain.Model.ValueObjects;

namespace Vitalita.API.IAM.Domain.Model.Queries;

/// <summary>
///     Query to get the users with a given email (zero or one, because the email is unique).
/// </summary>
/// <param name="Email">The email to look for.</param>
public record GetUsersByEmailQuery(EmailAddress Email);
