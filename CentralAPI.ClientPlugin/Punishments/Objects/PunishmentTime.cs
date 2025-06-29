using CentralAPI.SharedLib;

using NetworkLib;

namespace CentralAPI.ClientPlugin.Punishments.Objects;

/// <summary>
/// Contains details about a punishment's duration and issuance time.
/// </summary>
public class PunishmentTime
{
    /// <summary>
    /// Whether or not the punishment is permanent.
    /// </summary>
    public bool IsPermanent { get; set; }
    
    /// <summary>
    /// Whether or not the punishment has expired.
    /// </summary>
    public bool IsExpired { get; set; }
    
    /// <summary>
    /// Gets or sets the time of the punishment being issued (in UTC).
    /// </summary>
    public DateTime UtcIssued { get; set; } = DateTime.MinValue;
    
    /// <summary>
    /// Gets or sets the time of the punishment start (in UTC).
    /// </summary>
    public DateTime UtcStart { get; set; } = DateTime.MinValue;
    
    /// <summary>
    /// Gets or sets the duration of the punishment.
    /// </summary>
    public TimeSpan Duration { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Gets the time of the punishment expiring (in UTC).
    /// <remarks>Returns <see cref="DateTime.MaxValue"/> for permanent punishments and <see cref="DateTime.MinValue"/> for expired punishments.</remarks>
    /// </summary>
    public DateTime UtcExpires
    {
        get
        {
            if (IsPermanent)
                return DateTime.MaxValue;
            
            if (IsExpired)
                return DateTime.MinValue;

            return UtcStart + Duration;
        }
    }
    
    /// <summary>
    /// Writes the data.
    /// </summary>
    /// <param name="writer">The target writer.</param>
    public void Write(NetworkWriter writer)
    {
        writer.WriteBool(IsPermanent);
        writer.WriteBool(IsExpired);
        writer.WriteDate(UtcIssued);
        writer.WriteDate(UtcStart);
        writer.WriteTime(Duration);
    }
    
    /// <summary>
    /// Reads the data.
    /// </summary>
    /// <param name="reader">The target reader.</param>
    public void Read(NetworkReader reader)
    {
        IsPermanent = reader.ReadBool();
        IsExpired = reader.ReadBool();
        UtcIssued = reader.ReadDate();
        UtcStart = reader.ReadDate();
        Duration = reader.ReadTime();
    }
}