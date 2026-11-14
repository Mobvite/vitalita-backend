namespace Vitalita.API.Shared.Infrastructure.Interfaces.ASP.Security;

/// <summary>
///     Marks an anonymous action whose behavior changes when a Bearer token is sent
///     (for example, <c>GET /users</c>: public with <c>?email=</c>, administrators only without it).
/// </summary>
/// <remarks>
///     It does not change the authorization. It only tells <c>BearerSecuritySchemeTransformer</c> to document the
///     Bearer scheme as optional, so Swagger UI sends the token when the user has clicked Authorize.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class OptionalBearerAuthenticationAttribute : Attribute;
