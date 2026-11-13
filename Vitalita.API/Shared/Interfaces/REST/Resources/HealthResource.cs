namespace Vitalita.API.Shared.Interfaces.REST.Resources;

/// <summary>
///     Liveness response of the API.
/// </summary>
/// <param name="Status">Always <c>Healthy</c> when the process can answer requests.</param>
public record HealthResource(string Status)
{
    /// <summary>
    ///     The only response: the process is up.
    /// </summary>
    public static readonly HealthResource Healthy = new("Healthy");
}
