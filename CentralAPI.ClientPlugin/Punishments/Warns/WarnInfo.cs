using CentralAPI.ClientPlugin.Punishments.Objects;

using NetworkLib;

namespace CentralAPI.ClientPlugin.Punishments.Warns;

public class WarnInfo : PunishmentInfo
{
    /// <summary>
    /// Whether or not the warn was displayed to the target player.
    /// </summary>
    public bool IsDisplayed { get; set; }

    /// <inheritdoc cref="PunishmentInfo.Read"/>>
    public override void Read(NetworkReader reader)
    {
        base.Read(reader);
        
        IsDisplayed = reader.ReadBool();
    }

    /// <inheritdoc cref="PunishmentInfo.Write"/>>
    public override void Write(NetworkWriter writer)
    {
        base.Write(writer);
        
        writer.WriteBool(IsDisplayed);
    }
}