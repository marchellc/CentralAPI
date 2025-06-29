using NetworkLib;

namespace CentralAPI.ClientPlugin.Punishments.Objects.Logs;

/// <summary>
/// Provides information about a punishment reason update.
/// </summary>
public class ReasonUpdateLog : PunishmentLog
{
    /// <inheritdoc cref="PunishmentLog.LogType"/>>
    public override byte LogType { get; } = 0;

    /// <summary>
    /// Gets or sets the new reason.
    /// </summary>
    public string NewReason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the previous reason.
    /// </summary>
    public string PreviousReason { get; set; } = string.Empty;

    /// <inheritdoc cref="PunishmentLog.Read"/>>
    public override void Read(NetworkReader reader)
    {
        NewReason = reader.ReadString();
        PreviousReason = reader.ReadString();
    }

    /// <inheritdoc cref="PunishmentLog.Write"/>>
    public override void Write(NetworkWriter writer)
    {
        writer.WriteString(NewReason);
        writer.WriteString(PreviousReason);
    }
}