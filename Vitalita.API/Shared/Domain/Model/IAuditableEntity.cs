namespace Vitalita.API.Shared.Domain.Model;

/// <summary>
///     Marks an entity as carrying audit timestamps managed by the persistence layer.
/// </summary>
/// <remarks>
///     <para>
///         Any entity in any bounded context that implements this interface gets <see cref="CreatedAt" /> set once on
///         first persistence and <see cref="UpdatedAt" /> refreshed on every save, via <c>AuditableEntityInterceptor</c>.
///     </para>
///     <para>
///         Both values are UTC instants stored as <see cref="DateTime" /> with <see cref="DateTimeKind.Utc" />.
///         <see cref="DateTime" /> is used instead of <see cref="DateTimeOffset" /> because MySQL has no column type
///         that keeps an offset, and because System.Text.Json writes a UTC <see cref="DateTime" /> with the
///         <c>"Z"</c> suffix the frontend contract expects (a <see cref="DateTimeOffset" /> is written as <c>"+00:00"</c>).
///     </para>
/// </remarks>
public interface IAuditableEntity
{
    /// <summary>
    ///     Gets or sets the UTC instant when the entity was first persisted.
    /// </summary>
    DateTime? CreatedAt { get; set; }

    /// <summary>
    ///     Gets or sets the UTC instant when the entity was last saved.
    /// </summary>
    DateTime? UpdatedAt { get; set; }
}
