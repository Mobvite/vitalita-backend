using Vitalita.API.Profiles.Domain.Model.ValueObjects;
using Vitalita.API.Profiles.Infrastructure.Security;

namespace Vitalita.API.Tests.Profiles.Infrastructure;

public class InvitationCodeGeneratorTests
{
    private readonly InvitationCodeGenerator _generator = new();

    [Fact]
    public void Generate_ReturnsVitaPrefixAndEightCharactersOfTheAlphabet()
    {
        for (var i = 0; i < 200; i++)
        {
            var code = _generator.Generate().Value;

            Assert.True(InvitationCode.HasGeneratedFormat(code), code);
        }
    }

    [Fact]
    public void Generate_ProducesDifferentCodes()
    {
        var codes = Enumerable.Range(0, 1000).Select(_ => _generator.Generate().Value).ToHashSet();

        // 32^8 combinations: 1000 draws practically never collide.
        Assert.True(codes.Count >= 999);
    }
}
