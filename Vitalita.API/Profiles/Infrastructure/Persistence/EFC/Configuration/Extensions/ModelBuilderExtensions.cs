using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitalita.API.Profiles.Domain.Model.Aggregates;
using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Shared.Infrastructure.Persistence.EFC.Configuration.Converters;

namespace Vitalita.API.Profiles.Infrastructure.Persistence.EFC.Configuration.Extensions;

/// <summary>
///     Fluent API configuration of the Profiles bounded context.
/// </summary>
/// <remarks>
///     Tables: <c>caregiver_profiles</c>, <c>family_member_profiles</c>, <c>older_adults</c> and
///     <c>family_accesses</c>. <c>user_id</c> references IAM by value only (no foreign key across contexts).
///     Lists of strings are stored as JSON arrays.
/// </remarks>
public static class ModelBuilderExtensions
{
    private const string JsonColumnType = "json";

    /// <summary>
    ///     Configures the Profiles aggregates.
    /// </summary>
    /// <param name="builder">The model builder.</param>
    public static void ApplyProfilesConfiguration(this ModelBuilder builder)
    {
        ConfigureCaregiverProfile(builder.Entity<CaregiverProfile>());
        ConfigureFamilyMemberProfile(builder.Entity<FamilyMemberProfile>());
        ConfigureOlderAdult(builder.Entity<OlderAdult>());
        ConfigureFamilyAccess(builder.Entity<FamilyAccess>());
    }

    private static void ConfigureCaregiverProfile(EntityTypeBuilder<CaregiverProfile> entity)
    {
        entity.HasKey(profile => profile.Id);
        entity.Property(profile => profile.Id).IsRequired().ValueGeneratedOnAdd();
        entity.Property(profile => profile.UserId).IsRequired();
        entity.HasIndex(profile => profile.UserId).IsUnique();
        entity.Property(profile => profile.FirstName).HasMaxLength(ProfileLimits.NameMaxLength).IsRequired();
        entity.Property(profile => profile.LastName).HasMaxLength(ProfileLimits.NameMaxLength).IsRequired();
        entity.Property(profile => profile.PhoneNumber).HasMaxLength(ProfileLimits.PhoneNumberMaxLength).IsRequired();
        entity.Property(profile => profile.ProfessionalTitle)
            .HasMaxLength(ProfileLimits.ProfessionalTitleMaxLength).IsRequired();
        entity.Property(profile => profile.Bio).HasMaxLength(ProfileLimits.BioMaxLength).IsRequired();
        entity.Property(profile => profile.PhotoUrl).HasMaxLength(ProfileLimits.PhotoUrlMaxLength).IsRequired();
        ConfigureStringList(entity.Property(profile => profile.Certifications));
    }

    private static void ConfigureFamilyMemberProfile(EntityTypeBuilder<FamilyMemberProfile> entity)
    {
        entity.HasKey(profile => profile.Id);
        entity.Property(profile => profile.Id).IsRequired().ValueGeneratedOnAdd();
        entity.Property(profile => profile.UserId).IsRequired();
        entity.HasIndex(profile => profile.UserId).IsUnique();
        entity.Property(profile => profile.FirstName).HasMaxLength(ProfileLimits.NameMaxLength).IsRequired();
        entity.Property(profile => profile.LastName).HasMaxLength(ProfileLimits.NameMaxLength).IsRequired();
        entity.Property(profile => profile.PhoneNumber).HasMaxLength(ProfileLimits.PhoneNumberMaxLength).IsRequired();
    }

    private static void ConfigureOlderAdult(EntityTypeBuilder<OlderAdult> entity)
    {
        entity.HasKey(olderAdult => olderAdult.Id);
        entity.Property(olderAdult => olderAdult.Id).IsRequired().ValueGeneratedOnAdd();
        entity.Property(olderAdult => olderAdult.CaregiverId).IsRequired();
        entity.HasOne<CaregiverProfile>().WithMany().HasForeignKey(olderAdult => olderAdult.CaregiverId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.Property(olderAdult => olderAdult.FirstName).HasMaxLength(ProfileLimits.NameMaxLength).IsRequired();
        entity.Property(olderAdult => olderAdult.LastName).HasMaxLength(ProfileLimits.NameMaxLength).IsRequired();
        entity.Property(olderAdult => olderAdult.BirthDate).HasColumnType("date").IsRequired();
        entity.Property(olderAdult => olderAdult.DocumentNumber)
            .HasMaxLength(ProfileLimits.DocumentNumberLength).IsRequired();
        entity.Property(olderAdult => olderAdult.BloodType).HasMaxLength(ProfileLimits.BloodTypeMaxLength).IsRequired();
        ConfigureStringList(entity.Property(olderAdult => olderAdult.Allergies));
        ConfigureStringList(entity.Property(olderAdult => olderAdult.ChronicConditions));
        entity.ComplexProperty(olderAdult => olderAdult.EmergencyContact, contact =>
        {
            contact.Property(value => value.Name).HasColumnName("emergency_contact_name")
                .HasMaxLength(ProfileLimits.NameMaxLength).IsRequired();
            contact.Property(value => value.PhoneNumber).HasColumnName("emergency_contact_phone_number")
                .HasMaxLength(ProfileLimits.PhoneNumberMaxLength).IsRequired();
            contact.Property(value => value.Relationship).HasColumnName("emergency_contact_relationship")
                .HasConversion<string>().HasMaxLength(ProfileLimits.EnumMaxLength);
        });
        entity.Property(olderAdult => olderAdult.Observations)
            .HasMaxLength(ProfileLimits.ObservationsMaxLength).IsRequired();
        entity.Property(olderAdult => olderAdult.Status)
            .HasConversion<string>().HasMaxLength(ProfileLimits.EnumMaxLength).IsRequired();
    }

    private static void ConfigureFamilyAccess(EntityTypeBuilder<FamilyAccess> entity)
    {
        entity.HasKey(access => access.Id);
        entity.Property(access => access.Id).IsRequired().ValueGeneratedOnAdd();
        entity.Property(access => access.OlderAdultId).IsRequired();
        entity.HasOne<OlderAdult>().WithMany().HasForeignKey(access => access.OlderAdultId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<FamilyMemberProfile>().WithMany().HasForeignKey(access => access.FamilyMemberId)
            .IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        entity.Property(access => access.InvitedEmail).HasMaxLength(ProfileLimits.EmailMaxLength).IsRequired();
        entity.HasIndex(access => access.InvitedEmail);
        entity.Property(access => access.InvitationCode)
            .HasConversion(code => code.Value, value => new InvitationCode(value))
            .HasMaxLength(InvitationCode.MaxLength)
            .IsRequired();
        entity.HasIndex(access => access.InvitationCode).IsUnique();
        entity.Property(access => access.Relationship)
            .HasConversion<string>().HasMaxLength(ProfileLimits.EnumMaxLength).IsRequired();
        entity.Property(access => access.Status)
            .HasConversion<string>().HasMaxLength(ProfileLimits.EnumMaxLength).IsRequired();
        entity.Property(access => access.InvitedAt).IsRequired();
        entity.Property(access => access.AcceptedAt);
        entity.Property(access => access.ExpiresAt).IsRequired();
        entity.Ignore(access => access.IsActive);
    }

    private static void ConfigureStringList(PropertyBuilder<IReadOnlyList<string>> property) =>
        property.HasConversion(new StringListJsonConverter(), StringListJsonConverter.Comparer)
            .HasColumnType(JsonColumnType)
            .IsRequired();
}
