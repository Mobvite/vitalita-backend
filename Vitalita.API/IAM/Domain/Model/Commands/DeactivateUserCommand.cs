namespace Vitalita.API.IAM.Domain.Model.Commands;

/// <summary>
///     Command to deactivate an account (logical delete).
/// </summary>
/// <param name="UserId">Identifier of the account to deactivate.</param>
/// <param name="RequesterId">Identifier of the administrator that sends the request.</param>
public record DeactivateUserCommand(int UserId, int RequesterId);
