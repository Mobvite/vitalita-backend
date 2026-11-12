namespace Vitalita.API.Profiles.Domain.Model.Exceptions;

/// <summary>
///     Thrown when a family access change is not allowed (a revoked access is final, an active access cannot go back to pending).
/// </summary>
public class InvalidAccessStatusTransitionException() : InvalidOperationException("The family access status change is not allowed.");
