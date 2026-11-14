namespace Vitalita.API.Shared.Domain.Services;

/// <summary>
///     Abstraction over the system clock.
/// </summary>
/// <remarks>
///     Every piece of server code that needs "now" or "today in Peru" (audit timestamps, dashboard,
///     "today" filters, invitation expiry) must use this abstraction instead of <see cref="DateTime.UtcNow" />,
///     so the behavior can be tested with a fixed clock. See <see cref="PeruTime" /> and
///     <see cref="ClockExtensions" /> for the Peru helpers.
/// </remarks>
public interface IClock
{
    /// <summary>
    ///     Gets the current instant in UTC (<see cref="DateTimeKind.Utc" />).
    /// </summary>
    DateTime UtcNow { get; }
}
