using Vitalita.API.Monitoring.Domain.Model.Exceptions;
using Vitalita.API.Monitoring.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Domain.Model;

namespace Vitalita.API.Monitoring.Domain.Model.Aggregates;

/// <summary>
///     Daily report aggregate root: how the older adult was during the day (US13).
/// </summary>
/// <remarks>
///     <list type="bullet">
///         <item><description><see cref="RecordedByUserId" /> is the author (from the token) and never changes.</description></item>
///         <item><description>The report instant cannot be in the future (5 minutes of clock skew are accepted).</description></item>
///         <item><description>The observations are required; several reports per day are allowed.</description></item>
///         <item><description>The older adult of a report never changes.</description></item>
///     </list>
/// </remarks>
public class DailyReport : IAuditableEntity
{
    /// <summary>
    ///     Protected parameterless constructor for EF Core.
    /// </summary>
    protected DailyReport()
    {
        Observations = string.Empty;
    }

    /// <summary>
    ///     Records a daily report.
    /// </summary>
    /// <param name="olderAdultId">Older adult of the report.</param>
    /// <param name="recordedByUserId">IAM user that writes the report.</param>
    /// <param name="reportDate">UTC instant of the report.</param>
    /// <param name="mood">Mood of the older adult.</param>
    /// <param name="generalCondition">General condition of the older adult.</param>
    /// <param name="observations">Observations (required).</param>
    /// <param name="utcNow">The current UTC instant.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an identifier is not positive.</exception>
    /// <exception cref="ArgumentException">Thrown when the observations are empty.</exception>
    /// <exception cref="InstantInFutureException">Thrown when the report instant is in the future.</exception>
    public DailyReport(int olderAdultId, int recordedByUserId, DateTime reportDate, Mood mood,
        GeneralCondition generalCondition, string observations, DateTime utcNow) : this()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(olderAdultId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(recordedByUserId);
        OlderAdultId = olderAdultId;
        RecordedByUserId = recordedByUserId;
        SetData(reportDate, mood, generalCondition, observations, utcNow);
    }

    /// <summary>Gets the server-generated identifier.</summary>
    public int Id { get; private set; }

    /// <summary>Gets the older adult of the report.</summary>
    public int OlderAdultId { get; private set; }

    /// <summary>Gets the IAM user that wrote the report.</summary>
    public int RecordedByUserId { get; private set; }

    /// <summary>Gets the UTC instant of the report.</summary>
    public DateTime ReportDate { get; private set; }

    /// <summary>Gets the mood of the older adult.</summary>
    public Mood Mood { get; private set; }

    /// <summary>Gets the general condition of the older adult.</summary>
    public GeneralCondition GeneralCondition { get; private set; }

    /// <summary>Gets the observations.</summary>
    public string Observations { get; private set; }

    /// <inheritdoc />
    public DateTime? CreatedAt { get; set; }

    /// <inheritdoc />
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    ///     Replaces the report data. The author is kept.
    /// </summary>
    /// <param name="olderAdultId">The older adult sent by the client; it must be the current one.</param>
    /// <param name="reportDate">UTC instant of the report.</param>
    /// <param name="mood">Mood of the older adult.</param>
    /// <param name="generalCondition">General condition of the older adult.</param>
    /// <param name="observations">Observations (required).</param>
    /// <param name="utcNow">The current UTC instant.</param>
    /// <exception cref="OlderAdultChangeNotAllowedException">Thrown when the older adult is another one.</exception>
    /// <exception cref="InstantInFutureException">Thrown when the report instant is in the future.</exception>
    public void Update(int olderAdultId, DateTime reportDate, Mood mood, GeneralCondition generalCondition,
        string observations, DateTime utcNow)
    {
        if (olderAdultId != OlderAdultId) throw new OlderAdultChangeNotAllowedException();
        SetData(reportDate, mood, generalCondition, observations, utcNow);
    }

    private void SetData(DateTime reportDate, Mood mood, GeneralCondition generalCondition, string observations,
        DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(observations);
        ReportDate = RecordedInstant.Validate(reportDate, utcNow);
        Mood = mood;
        GeneralCondition = generalCondition;
        Observations = observations.Trim();
    }
}
