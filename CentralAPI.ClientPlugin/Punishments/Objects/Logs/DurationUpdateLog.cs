using CentralAPI.SharedLib;

using NetworkLib;

namespace CentralAPI.ClientPlugin.Punishments.Objects.Logs;

/// <summary>
/// Provides information about a duration update.
/// </summary>
public class DurationUpdateLog : PunishmentLog
{
    /// <inheritdoc cref="PunishmentLog.LogType"/>>
    public override byte LogType { get; } = 1;
    
    /// <summary>
    /// Gets or sets the new value of <see cref="PunishmentTime.IsExpired"/>
    /// </summary>
    public bool NewIsExpired { get; set; }
    
    /// <summary>
    /// Gets or sets the previous value of <see cref="PunishmentTime.IsExpired"/>
    /// </summary>
    public bool PreviousIsExpired { get; set; }
    
    /// <summary>
    /// Gets or sets the new value of <see cref="PunishmentTime.IsPermanent"/>
    /// </summary>
    public bool NewIsPermanent { get; set; }
    
    /// <summary>
    /// Gets or sets the previous value of <see cref="PunishmentTime.IsPermanent"/>
    /// </summary>
    public bool PreviousIsPermanent { get; set; }
    
    /// <summary>
    /// Gets or sets the new UTC start time.
    /// </summary>
    public DateTime? NewUtcStart { get; set; }
    
    /// <summary>
    /// Gets or sets the previous UTC start time.
    /// </summary>
    public DateTime? PreviousUtcStart { get; set; }
    
    /// <summary>
    /// Gets or sets the new duration.
    /// </summary>
    public TimeSpan? NewDuration { get; set; }
    
    /// <summary>
    /// Gets or sets the previous duration.
    /// </summary>
    public TimeSpan? PreviousDuration { get; set; }
    
    /// <inheritdoc cref="PunishmentLog.Read"/>>
    public override void Read(NetworkReader reader)
    {
        NewIsExpired = reader.ReadBool();
        PreviousIsExpired = reader.ReadBool();

        NewIsPermanent = reader.ReadBool();
        PreviousIsPermanent = reader.ReadBool();

        if (reader.ReadBool())
        {
            NewUtcStart = reader.ReadDate();
            PreviousUtcStart = reader.ReadDate();
        }

        if (reader.ReadBool())
        {
            NewDuration = reader.ReadTime();
            PreviousDuration = reader.ReadTime();
        }
    }

    /// <inheritdoc cref="PunishmentLog.Write"/>>
    public override void Write(NetworkWriter writer)
    {
        writer.WriteBool(NewIsExpired);
        writer.WriteBool(PreviousIsExpired);
        
        writer.WriteBool(NewIsPermanent);
        writer.WriteBool(PreviousIsPermanent);

        if (NewUtcStart.HasValue)
        {
            writer.WriteDate(NewUtcStart.Value);
            writer.WriteDate(PreviousUtcStart.Value);
        }

        if (NewDuration.HasValue)
        {
            writer.WriteTime(NewDuration.Value);
            writer.WriteTime(PreviousDuration.Value);
        }
    }
}