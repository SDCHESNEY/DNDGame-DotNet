namespace DNDGame.Core.Models;

/// <summary>
/// Represents the context of a location for scene descriptions.
/// </summary>
public record LocationContext(
    string Name,
    string LocationType,
    string? Description,
    List<string> VisibleFeatures,
    List<string> PresentNpcs,
    Dictionary<string, object> AdditionalDetails)
{
    /// <summary>
    /// Gets whether the location has a description.
    /// </summary>
    public bool HasDescription => !string.IsNullOrEmpty(Description);

    /// <summary>
    /// Gets whether there are any visible features.
    /// </summary>
    public bool HasFeatures => VisibleFeatures.Any();

    /// <summary>
    /// Gets whether there are NPCs present.
    /// </summary>
    public bool HasNpcs => PresentNpcs.Any();

    /// <summary>
    /// Gets the number of visible features.
    /// </summary>
    public int FeatureCount => VisibleFeatures.Count;

    /// <summary>
    /// Gets the number of NPCs present.
    /// </summary>
    public int NpcCount => PresentNpcs.Count;

    /// <summary>
    /// Gets additional detail by key.
    /// </summary>
    public T? GetDetail<T>(string key) where T : class
    {
        return AdditionalDetails.TryGetValue(key, out var value) ? value as T : null;
    }

    /// <summary>
    /// Creates a simple location context with minimal information.
    /// </summary>
    public static LocationContext Simple(string name, string locationType)
    {
        return new LocationContext(
            name,
            locationType,
            null,
            new List<string>(),
            new List<string>(),
            new Dictionary<string, object>());
    }
}
