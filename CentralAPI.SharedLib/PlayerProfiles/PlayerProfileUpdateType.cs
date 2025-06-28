namespace CentralAPI.SharedLib.PlayerProfiles;

/// <summary>
/// Specifies what to update.
/// </summary>
[Flags]
public enum PlayerProfileUpdateType : byte
{
    /// <summary>
    /// No updates.
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Update the profile's activity.
    /// </summary>
    Activity = 1,
    
    /// <summary>
    /// Update the profile's address cache.
    /// </summary>
    Address = 2,
    
    /// <summary>
    /// Update the profile's username cache.
    /// </summary>
    Username = 4,
    
    /// <summary>
    /// Update a specific profile property.
    /// </summary>
    Property = 8,
    
    /// <summary>
    /// Create a player profile.
    /// </summary>
    Create = 16,
    
    /// <summary>
    /// Delete a player profile
    /// </summary>
    Delete = 32
}