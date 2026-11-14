using Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Validation;

namespace Vitalita.API.Tests.Shared.Infrastructure.Interfaces.ASP.Validation;

public class ItemsMaxLengthAttributeTests
{
    private readonly ItemsMaxLengthAttribute _attribute = new(5) { ErrorMessage = "ClinicalListLimit" };

    [Fact]
    public void NullOrShortItems_AreValid()
    {
        Assert.True(_attribute.IsValid(null));
        Assert.True(_attribute.IsValid(new List<string>()));
        Assert.True(_attribute.IsValid(new[] { "abcde", "  abc  " }));
    }

    [Fact]
    public void ALongItem_IsInvalid()
    {
        Assert.False(_attribute.IsValid(new[] { "abc", "abcdef" }));
    }
}
