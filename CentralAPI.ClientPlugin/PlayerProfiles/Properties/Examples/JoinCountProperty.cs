using CentralAPI.ClientPlugin.PlayerProfiles.Internal;

using LabExtended.API;

using NetworkLib;

namespace CentralAPI.ClientPlugin.PlayerProfiles.Properties.Examples;

/// <summary>
/// An example property that logs joins.
/// </summary>
public class JoinCountProperty : CustomPlayerProfileProperty<int>
{
    /// <inheritdoc cref="PlayerProfilePropertyBase.Read"/>
    public override void OnJoined(ExPlayer player)
    {
        base.OnJoined(player);

        Value++;
    }

    /// <inheritdoc cref="PlayerProfilePropertyBase.Read"/>
    public override void Read(NetworkReader reader, bool isNew)
    {
        ObjectValue = reader.ReadInt();
    }

    /// <inheritdoc cref="PlayerProfilePropertyBase.Write"/>
    public override void Write(NetworkWriter writer)
    {
        writer.WriteInt(Value);
    }
}