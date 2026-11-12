namespace Vitalita.API.Monitoring.Domain.Model.ValueObjects;

/// <summary>
///     Lengths, sizes, formats and tolerances shared by the Monitoring aggregates, resources and columns.
/// </summary>
public static class MonitoringLimits
{
    /// <summary>Maximum length of the daily report observations.</summary>
    public const int ObservationsMaxLength = 1000;

    /// <summary>Maximum length of a medication name.</summary>
    public const int MedicationNameMaxLength = 100;

    /// <summary>Maximum length of a dosage.</summary>
    public const int DosageMaxLength = 50;

    /// <summary>Maximum length of a frequency.</summary>
    public const int FrequencyMaxLength = 100;

    /// <summary>Maximum length of the medication instructions.</summary>
    public const int InstructionsMaxLength = 500;

    /// <summary>Maximum number of schedule times of a medication.</summary>
    public const int ScheduleTimesMaxCount = 12;

    /// <summary>A wall-clock time <c>HH:mm</c> from 00:00 to 23:59 (same rule as the frontend).</summary>
    public const string ScheduleTimePattern = @"^([01]\d|2[0-3]):[0-5]\d$";

    /// <summary>Maximum length of the unit column.</summary>
    public const int UnitMaxLength = 10;

    /// <summary>Maximum length of the enum columns.</summary>
    public const int EnumMaxLength = 20;

    /// <summary>Precision of the vital sign values (<c>decimal(6,2)</c>).</summary>
    public const int ValuePrecision = 6;

    /// <summary>Scale of the vital sign values (<c>decimal(6,2)</c>).</summary>
    public const int ValueScale = 2;

    /// <summary>Tolerance for client clocks: an instant can be at most this far in the future.</summary>
    public static readonly TimeSpan MaxClockSkew = TimeSpan.FromMinutes(5);
}
