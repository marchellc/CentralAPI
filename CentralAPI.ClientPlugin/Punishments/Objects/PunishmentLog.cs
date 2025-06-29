using System.Text;

using NetworkLib;

namespace CentralAPI.ClientPlugin.Punishments.Objects;

/// <summary>
/// Contains information about a punishment update.
/// </summary>
public abstract class PunishmentLog
{
    /// <summary>
    /// Gets the type of the log.
    /// </summary>
    public abstract byte LogType { get; }
    
    /// <summary>
    /// Whether or not the update was made by the director.
    /// </summary>
    public bool IsDirector { get; set; }

    /// <summary>
    /// Gets or sets the alias of the server on which the update was made.
    /// </summary>
    public string Server { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the update reason.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the time of the update.
    /// </summary>
    public DateTime Time { get; set; }
    
    /// <summary>
    /// Gets or sets the player who made the update.
    /// </summary>
    public PunishmentPlayer? Creator { get; set; }
    
    /// <summary>
    /// Reads the data.
    /// </summary>
    /// <param name="reader">The target reader.</param>
    public abstract void Read(NetworkReader reader);
    
    /// <summary>
    /// Writes the data.
    /// </summary>
    /// <param name="writer">The target writer.</param>
    public abstract void Write(NetworkWriter writer);
    
    /// <summary>
    /// Appends the data of the log to a specific builder.
    /// </summary>
    /// <param name="builder">The target builder.</param>
    public virtual void AppendLog(StringBuilder builder) { }
}