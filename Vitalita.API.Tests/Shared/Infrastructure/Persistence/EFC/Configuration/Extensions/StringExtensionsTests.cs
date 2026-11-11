using Vitalita.API.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

namespace Vitalita.API.Tests.Shared.Infrastructure.Persistence.EFC.Configuration.Extensions;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("OlderAdult", "older_adult")]
    [InlineData("createdAt", "created_at")]
    [InlineData("FamilyMemberId", "family_member_id")]
    [InlineData("IX_OlderAdults_CaregiverId", "ix_older_adults_caregiver_id")]
    [InlineData("PK_Users", "pk_users")]
    [InlineData("FK_ExamEvidences_MedicalExams_ExamId", "fk_exam_evidences_medical_exams_exam_id")]
    [InlineData("id", "id")]
    [InlineData("", "")]
    public void ToSnakeCase_ConvertsPascalAndCamelCase(string input, string expected)
    {
        Assert.Equal(expected, input.ToSnakeCase());
    }

    [Theory]
    [InlineData("OlderAdult", "OlderAdults")]
    [InlineData("FamilyAccess", "FamilyAccesses")]
    [InlineData("MedicationAdministration", "MedicationAdministrations")]
    [InlineData("Status", "Statuses")]
    [InlineData("Reminder", "Reminders")]
    [InlineData("Users", "Users")]
    public void ToPlural_PluralizesEnglishNames(string input, string expected)
    {
        Assert.Equal(expected, input.ToPlural());
    }

    [Fact]
    public void ToPluralThenToSnakeCase_ProducesTheTableName()
    {
        Assert.Equal("caregiver_profiles", "CaregiverProfile".ToPlural().ToSnakeCase());
    }
}
