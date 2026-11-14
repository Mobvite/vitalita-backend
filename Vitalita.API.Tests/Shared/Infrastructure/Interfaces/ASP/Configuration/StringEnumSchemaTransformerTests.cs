using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Configuration;

namespace Vitalita.API.Tests.Shared.Infrastructure.Interfaces.ASP.Configuration;

public class StringEnumSchemaTransformerTests
{
    private enum SampleStatus
    {
        Active,
        Inactive
    }

    private static OpenApiSchema EnumSchema() => new()
    {
        Enum = [JsonValue.Create("Active"), JsonValue.Create("Inactive")]
    };

    private static string[] Values(OpenApiSchema schema) =>
        schema.Enum!.Select(value => value!.GetValue<string>()).ToArray();

    [Fact]
    public void Apply_OnEnum_SetsStringTypeAndKeepsValues()
    {
        var schema = EnumSchema();

        StringEnumSchemaTransformer.Apply(schema, typeof(SampleStatus));

        Assert.Equal(JsonSchemaType.String, schema.Type);
        Assert.Equal(["Active", "Inactive"], Values(schema));
    }

    [Fact]
    public void Apply_OnNullableEnum_SetsStringType()
    {
        var schema = EnumSchema();

        StringEnumSchemaTransformer.Apply(schema, typeof(SampleStatus?));

        Assert.Equal(JsonSchemaType.String, schema.Type);
    }

    [Fact]
    public void Apply_OnNonEnumType_DoesNotChangeTheSchema()
    {
        var schema = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" };

        StringEnumSchemaTransformer.Apply(schema, typeof(int));

        Assert.Equal(JsonSchemaType.Integer, schema.Type);
        Assert.Null(schema.Enum);
    }

    [Fact]
    public void Apply_OnEnumWithExistingType_KeepsTheType()
    {
        var schema = EnumSchema();
        schema.Type = JsonSchemaType.String | JsonSchemaType.Null;

        StringEnumSchemaTransformer.Apply(schema, typeof(SampleStatus?));

        Assert.Equal(JsonSchemaType.String | JsonSchemaType.Null, schema.Type);
    }
}
