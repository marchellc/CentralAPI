using CentralAPI.SharedLib.PlayerProfiles;

using LabExtended.API;

using NetworkLib;
using Utils.NonAllocLINQ;

namespace CentralAPI.ClientPlugin.PlayerProfiles.Internal;

/// <summary>
/// Base type for a profile property.
/// </summary>
public abstract class PlayerProfilePropertyBase
{
    /// <summary>
    /// Gets the name of the property.
    /// </summary>
    public string Name { get; internal set; } = string.Empty;
    
    /// <summary>
    /// Whether or not the value of the property has been assigned.
    /// </summary>
    public virtual bool IsAssigned { get; }
    
    /// <summary>
    /// Gets the current value of the property as an object.
    /// </summary>
    public abstract object ObjectValue { get; set; }
    
    /// <summary>
    /// Gets the parent profile.
    /// </summary>
    public PlayerProfileInstance Profile { get; internal set; }
    
    /// <summary>
    /// Gets the player assigned for this property.
    /// </summary>
    public ExPlayer Player => Profile.Player;

    /// <summary>
    /// Gets or sets the dirty value of the property.
    /// </summary>
    public bool IsDirty
    {
        get => field;
        set
        {
            if (field == value)
                return;
            
            field = value;

            if (value)
            {
                Profile.DirtyProperties.AddIfNotContains(this);
                Profile.DirtyFlags |= PlayerProfileUpdateType.Property;
            }
        }
    }

    /// <summary>
    /// Initializes the property.
    /// </summary>
    public virtual void Initialize() { }

    /// <summary>
    /// Destroys the property.
    /// </summary>
    public virtual void Destroy() { }

    /// <summary>
    /// Gets called when a player joins.
    /// </summary>
    /// <param name="player">The player who joined.</param>
    public virtual void OnJoined(ExPlayer player) { }
    
    /// <summary>
    /// Gets called when a player leaves.
    /// </summary>
    /// <param name="player">The player who left.</param>
    public virtual void OnLeft(ExPlayer player) { }

    /// <summary>
    /// Reads the data of the property.
    /// </summary>
    /// <param name="reader">The target reader.</param>
    /// <param name="isNew">Whether or not this is the first value.</param>
    public abstract void Read(NetworkReader reader, bool isNew);

    /// <summary>
    /// Writes the data of the property.
    /// </summary>
    /// <param name="writer">The target writer.</param>
    public abstract void Write(NetworkWriter writer);
}