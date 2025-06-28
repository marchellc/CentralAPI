using CentralAPI.SharedLib.PlayerProfiles;

using NetworkLib;

namespace CentralAPI.ServerApp.PlayerProfiles;

/// <summary>
/// A property of a player's profile.
/// </summary>
public class PlayerProfileProperty
{
    /// <summary>
    /// Gets the raw data of the property.
    /// </summary>
    public volatile byte[]? RawData;

    /// <summary>
    /// Gets the name of the property.
    /// </summary>
    public volatile string? Name;

    /// <summary>
    /// Gets the path to the file of the property.
    /// </summary>
    public volatile string? FilePath;
}