using Microsoft.EntityFrameworkCore;

namespace Vitalita.API.Shared.Application.Persistence;

/// <summary>
///     Helpers to classify the <see cref="DbUpdateException" /> thrown when changes are saved.
/// </summary>
public static class DbUpdateExceptionExtensions
{
    /// <summary>
    ///     MySQL error number of a duplicate key violation.
    /// </summary>
    private const int DuplicateKeyErrorNumber = 1062;

    /// <summary>
    ///     Determines whether the exception is a MySQL duplicate key violation (error 1062), for example a unique
    ///     index rejecting a concurrent insert.
    /// </summary>
    /// <remarks>The MySQL exception type is read by name, so the application layer does not reference the driver.</remarks>
    /// <param name="exception">The exception thrown by <c>SaveChanges</c>.</param>
    /// <returns><c>true</c> when an inner exception is a <c>MySqlException</c> with number 1062.</returns>
    public static bool IsDuplicateKeyViolation(this DbUpdateException exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (!string.Equals(current.GetType().Name, "MySqlException", StringComparison.Ordinal)) continue;
            var numberProperty = current.GetType().GetProperty("Number");
            if (numberProperty?.PropertyType == typeof(int) &&
                numberProperty.GetValue(current) is int errorCode &&
                errorCode == DuplicateKeyErrorNumber)
                return true;
        }

        return false;
    }
}
