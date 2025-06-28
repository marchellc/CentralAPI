using NetworkLib;
using NetworkLib.Interfaces;

namespace CentralAPI.SharedLib.PlayerProfiles;

/// <summary>
/// Used to update profile values.
/// </summary>
public struct PlayerProfileUpdateMessage : INetworkMessage
{
    /// <summary>
    /// Gets the type of the update.
    /// </summary>
    public PlayerProfileUpdateType Type;

    /// <summary>
    /// Gets the ID of the target profile.
    /// </summary>
    public string UserId;

    /// <summary>
    /// Gets the update data.
    /// </summary>
    public byte[] Data;

    /// <summary>
    /// Creates a new <see cref="PlayerProfileUpdateMessage"/> instance.
    /// </summary>
    /// <param name="userId">The user ID of the updated profile.</param>
    /// <param name="type">The target type.</param>
    /// <param name="data">The update data.</param>
    public PlayerProfileUpdateMessage(string userId, PlayerProfileUpdateType type, byte[] data)
    {
        UserId = userId;
        Type = type;
        Data = data;
    }
    
    /// <inheritdoc cref="INetworkMessage.Read"/>>
    public void Read(NetworkReader reader)
    {
        UserId = reader.ReadString();
        Type = (PlayerProfileUpdateType)reader.ReadByte();

        if (reader.Remaining > 0)
            Data = reader.ReadBytes();
    }

    /// <inheritdoc cref="INetworkMessage.Write"/>>
    public void Write(NetworkWriter writer)
    {
        writer.WriteString(UserId);
        writer.WriteByte((byte)Type);
        
        if (Data?.Length > 0)
            writer.WriteBytes(Data);
    }
}