using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitalita.API.Monitoring.Domain.Model.Aggregates;
using Vitalita.API.Monitoring.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Infrastructure.Persistence.EFC.Configuration.Converters;

namespace Vitalita.API.Monitoring.Infrastructure.Persistence.EFC.Configuration.Extensions;

/// <summary>
///     Fluent API configuration of the Monitoring bounded context.
/// </summary>
/// <remarks>
///     Tables: <c>daily_reports</c>, <c>vital_signs</c> and <c>medications</c>. <c>older_adult_id</c> and
///     <c>recorded_by_user_id</c> reference other contexts by value only (no foreign keys, indexes only).
/// </remarks>
public static class ModelBuilderExtensions
{
    /// <summary>
    ///     Configures the Monitoring aggregates.
    /// </summary>
    /// <param name="builder">The model builder.</param>
    public static void ApplyMonitoringConfiguration(this ModelBuilder builder)
    {
        ConfigureDailyReport(builder.Entity<DailyReport>());
        ConfigureVitalSign(builder.Entity<VitalSign>());
        ConfigureMedication(builder.Entity<Medication>());
    }

    private static void ConfigureDailyReport(EntityTypeBuilder<DailyReport> entity)
    {
        entity.HasKey(report => report.Id);
        entity.Property(report => report.Id).IsRequired().ValueGeneratedOnAdd();
        entity.Property(report => report.OlderAdultId).IsRequired();
        entity.HasIndex(report => report.OlderAdultId);
        entity.Property(report => report.RecordedByUserId).IsRequired();
        entity.Property(report => report.ReportDate).IsRequired();
        entity.Property(report => report.Mood)
            .HasConversion<string>().HasMaxLength(MonitoringLimits.EnumMaxLength).IsRequired();
        entity.Property(report => report.GeneralCondition)
            .HasConversion<string>().HasMaxLength(MonitoringLimits.EnumMaxLength).IsRequired();
        entity.Property(report => report.Observations)
            .HasMaxLength(MonitoringLimits.ObservationsMaxLength).IsRequired();
    }

    private static void ConfigureVitalSign(EntityTypeBuilder<VitalSign> entity)
    {
        entity.HasKey(sign => sign.Id);
        entity.Property(sign => sign.Id).IsRequired().ValueGeneratedOnAdd();
        entity.Property(sign => sign.OlderAdultId).IsRequired();
        entity.HasIndex(sign => sign.OlderAdultId);
        entity.Property(sign => sign.Type)
            .HasConversion<string>().HasMaxLength(MonitoringLimits.EnumMaxLength).IsRequired();
        entity.Property(sign => sign.Value)
            .HasPrecision(MonitoringLimits.ValuePrecision, MonitoringLimits.ValueScale).IsRequired();
        entity.Property(sign => sign.SecondaryValue)
            .HasPrecision(MonitoringLimits.ValuePrecision, MonitoringLimits.ValueScale);
        entity.Property(sign => sign.Unit).HasMaxLength(MonitoringLimits.UnitMaxLength).IsRequired();
        entity.Property(sign => sign.MeasuredAt).IsRequired();
    }

    private static void ConfigureMedication(EntityTypeBuilder<Medication> entity)
    {
        entity.HasKey(medication => medication.Id);
        entity.Property(medication => medication.Id).IsRequired().ValueGeneratedOnAdd();
        entity.Property(medication => medication.OlderAdultId).IsRequired();
        entity.HasIndex(medication => medication.OlderAdultId);
        entity.Property(medication => medication.Name)
            .HasMaxLength(MonitoringLimits.MedicationNameMaxLength).IsRequired();
        entity.Property(medication => medication.Dosage).HasMaxLength(MonitoringLimits.DosageMaxLength).IsRequired();
        entity.Property(medication => medication.Frequency)
            .HasMaxLength(MonitoringLimits.FrequencyMaxLength).IsRequired();
        entity.Property(medication => medication.ScheduleTimes)
            .HasConversion(new StringListJsonConverter(), StringListJsonConverter.Comparer)
            .HasColumnType("json")
            .IsRequired();
        entity.Property(medication => medication.Instructions)
            .HasMaxLength(MonitoringLimits.InstructionsMaxLength).IsRequired();
        entity.Property(medication => medication.Active).IsRequired();
    }
}
