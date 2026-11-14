using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Configuration;
using Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Security;

namespace Vitalita.API.Tests.Shared.Infrastructure.Interfaces.ASP.Configuration;

public class BearerSecuritySchemeTransformerTests
{
    private static OpenApiDocument DocumentWithScheme()
    {
        var document = new OpenApiDocument();
        BearerSecuritySchemeTransformer.ApplyToDocument(document, "JWT");
        return document;
    }

    [Fact]
    public void ApplyToDocument_AddsTheHttpBearerScheme()
    {
        var document = DocumentWithScheme();

        var scheme = document.Components!.SecuritySchemes![BearerSecuritySchemeTransformer.SchemeName];
        Assert.Equal(SecuritySchemeType.Http, scheme.Type);
        Assert.Equal("bearer", scheme.Scheme);
        Assert.Equal("JWT", scheme.BearerFormat);
    }

    [Fact]
    public void ApplyToOperation_WithAuthorize_RequiresBearer()
    {
        var operation = new OpenApiOperation();

        BearerSecuritySchemeTransformer.ApplyToOperation(operation, [new AuthorizeAttribute()], DocumentWithScheme());

        var requirement = Assert.Single(operation.Security!);
        Assert.Single(requirement);
    }

    [Fact]
    public void ApplyToOperation_WithAllowAnonymous_HasNoSecurity()
    {
        var operation = new OpenApiOperation();

        BearerSecuritySchemeTransformer.ApplyToOperation(operation,
            [new AuthorizeAttribute(), new AllowAnonymousAttribute()], DocumentWithScheme());

        Assert.Null(operation.Security);
    }

    [Fact]
    public void ApplyToOperation_WithOptionalBearer_AllowsAnonymousOrBearer()
    {
        var operation = new OpenApiOperation();

        BearerSecuritySchemeTransformer.ApplyToOperation(operation,
            [new AuthorizeAttribute(), new AllowAnonymousAttribute(), new OptionalBearerAuthenticationAttribute()],
            DocumentWithScheme());

        Assert.Equal(2, operation.Security!.Count);
        Assert.Empty(operation.Security[0]);
        Assert.Single(operation.Security[1]);
    }

    [Fact]
    public void ApplyToOperation_WithoutAuthorizationMetadata_HasNoSecurity()
    {
        var operation = new OpenApiOperation();

        BearerSecuritySchemeTransformer.ApplyToOperation(operation, [], DocumentWithScheme());

        Assert.Null(operation.Security);
    }
}
