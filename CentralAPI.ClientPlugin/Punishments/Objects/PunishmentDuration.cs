using NetworkLib;

namespace CentralAPI.ClientPlugin.Punishments.Objects;

/// <summary>
/// The duration of a punishment.
/// </summary>
public class PunishmentDuration
{
    /// <summary>
    /// Whether or not the punishment is permanent.
    /// </summary>
    public bool IsPermanent { get; internal set; }
    
    /// <summary>
    /// Whether or not the punishment has expired.
    /// </summary>
    public bool IsExpired { get; internal set; }
    
    /// <summary>
    /// The UTC time when the punishment was issued.
    /// </summary>
    public DateTime Issued { get; internal set; } = DateTime.MinValue;

    /// <summary>
    /// Gets the duration of the punishment.
    /// <remarks><see cref="TimeSpan.MaxValue"/> if the punishment is permanent.</remarks>
    /// </summary>
    public TimeSpan Duration { get; internal set; } = TimeSpan.Zero;

    /// <summary>
    /// The UTC time when the punishment expires.
    /// <remarks><see cref="DateTime.MaxValue"/> for permanent punishments, <see cref="DateTime.MinValue"/> for expired punishments.</remarks>
    /// </summary>
    public DateTime Expires
    {
        get
        {
            if (IsPermanent)
                return DateTime.MaxValue;
            
            if (IsExpired)
                return DateTime.MinValue;
            
            return Issued + Duration;
        }
    }

    /// <summary>
    /// Writes the duration data.
    /// </summary>
    /// <param name="writer">The target writer.</param>
    public void Write(NetworkWriter writer)
    {
        
    }

    /// <summary>
    /// Reads the duration data.
    /// </summary>
    /// <param name="reader">The target reader.</param>
    public void Read(NetworkReader reader)
    {
        
    }
}