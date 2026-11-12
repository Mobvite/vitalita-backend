namespace Vitalita.API.Profiles.Domain.Model.Exceptions;

/// <summary>
///     Thrown when the birth date of an older adult is later than today in Peru.
/// </summary>
public class BirthDateInFutureException() : InvalidOperationException("The birth date cannot be in the future.");
