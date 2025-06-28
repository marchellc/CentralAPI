using CentralAPI.ClientPlugin.Punishments.Objects;

using NetworkLib;

namespace CentralAPI.ClientPlugin.Punishments.Warns;

/// <summary>
/// A subtype of <see cref="PunishmentInfo"/> used specifically for warns.
/// </summary>
public class WarnPunishmentInfo : PunishmentInfo
{
    /// <inheritdoc cref="PunishmentInfo.CanExpire"/>
    public override bool CanExpire { get; } = true;

    /// <summary>
    /// Whether or not the warn was displayed to the target player.
    /// </summary>
    public bool WasDisplayed { get; internal set; }

    /// <inheritdoc cref="PunishmentInfo.Read"/>>
    public override void Read(NetworkReader reader)
    {
        base.Read(reader);
        
        WasDisplayed = reader.ReadBool();
    }

    /// <inheritdoc cref="PunishmentInfo.Write"/>>
    public override void Write(NetworkWriter writer)
    {
        base.Write(writer);
        
        writer.WriteBool(WasDisplayed);
    }
}