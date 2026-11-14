using Vitalita.API.Shared.Domain.Services;

namespace Vitalita.API.Shared.Infrastructure.Time;

/// <summary>
///     <see cref="IClock" /> implementation backed by the system clock.
/// </summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
