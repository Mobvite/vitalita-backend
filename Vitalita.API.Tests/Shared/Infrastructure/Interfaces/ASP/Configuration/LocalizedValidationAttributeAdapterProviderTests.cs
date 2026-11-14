using System.ComponentModel.DataAnnotations;
using Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Configuration;

namespace Vitalita.API.Tests.Shared.Infrastructure.Interfaces.ASP.Configuration;

public class LocalizedValidationAttributeAdapterProviderTests
{
    private readonly LocalizedValidationAttributeAdapterProvider _provider = new();

    [Fact]
    public void GetAttributeAdapter_ForAttributesWithoutBuiltInAdapter_ReturnsAnAdapter()
    {
        Assert.NotNull(_provider.GetAttributeAdapter(new AllowedValuesAttribute("A") { ErrorMessage = "Key" }, null));
        Assert.NotNull(_provider.GetAttributeAdapter(new DeniedValuesAttribute("A") { ErrorMessage = "Key" }, null));
    }

    [Fact]
    public void GetAttributeAdapter_ForBuiltInAttributes_KeepsTheDefaultAdapter()
    {
        var adapter = _provider.GetAttributeAdapter(new RequiredAttribute(), null);

        Assert.Equal("RequiredAttributeAdapter", adapter!.GetType().Name);
    }
}
