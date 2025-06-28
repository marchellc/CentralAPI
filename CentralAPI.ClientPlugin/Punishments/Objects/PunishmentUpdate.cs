namespace CentralAPI.ClientPlugin.Punishments.Objects;

/// <summary>
/// Provides information about a punishment update.
/// </summary>
public class PunishmentUpdate
{
    /// <summary>
    /// Gets or sets the type of the update.
    /// </summary>
    public byte Type { get; set; }
    
    /// <summary>
    /// Gets or sets the original value.
    /// </summary>
    public object? OriginalValue { get; set; }
    
    /// <summary>
    /// Gets or sets the new value.
    /// </summary>
    public object? NewValue { get; set; }
    
    /// <summary>
    /// Gets or sets the time of the update.
    /// </summary>
    public DateTime Time { get; set; }
    
    /// <summary>
    /// Gets or sets the player who caused the modification.
    /// </summary>
    public PunishmentPlayer Player { get; set; }
}