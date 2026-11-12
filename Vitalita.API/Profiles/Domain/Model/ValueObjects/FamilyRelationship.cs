namespace Vitalita.API.Profiles.Domain.Model.ValueObjects;

/// <summary>
///     Relationship of a person with an older adult. Values match the frontend enum.
/// </summary>
public enum FamilyRelationship
{
    /// <summary>Son.</summary>
    Son,

    /// <summary>Daughter.</summary>
    Daughter,

    /// <summary>Husband, wife or partner.</summary>
    Spouse,

    /// <summary>Grandson or granddaughter.</summary>
    Grandchild,

    /// <summary>Brother or sister.</summary>
    Sibling,

    /// <summary>Any other relationship.</summary>
    Other
}
