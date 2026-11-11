using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Configuration;

/// <summary>
///     Documents enums as strings in the OpenAPI document.
/// </summary>
/// <remarks>
///     Enums are serialized as strings (<c>JsonStringEnumConverter</c>), but the built-in generator emits only the
///     <c>enum</c> values without a <c>type</c>, so Swagger UI shows them as <c>any</c>. This transformer adds
///     <c>"type": "string"</c> to enum schemas and keeps their values. Other schemas are not changed.
/// </remarks>
public static class StringEnumSchemaTransformer
{
    /// <summary>
    ///     OpenAPI schema transformer entry point.
    /// </summary>
    /// <param name="schema">The schema being transformed.</param>
    /// <param name="context">The transformer context, used to get the CLR type of the schema.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public static Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        Apply(schema, context.JsonTypeInfo.Type);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Sets <c>"type": "string"</c> on the schema when <paramref name="type" /> is an enum (or a nullable enum)
    ///     whose schema lists its values and has no type yet.
    /// </summary>
    /// <param name="schema">The schema to update.</param>
    /// <param name="type">The CLR type described by the schema.</param>
    public static void Apply(OpenApiSchema schema, Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        if (!underlyingType.IsEnum || schema.Enum is not { Count: > 0 } || schema.Type is not null) return;

        schema.Type = JsonSchemaType.String;
    }
}
