using NetworkLib;
using NetworkLib.Interfaces;

namespace CentralAPI.SharedLib.PlayerProfiles;

/// <summary>
/// A message that contains all loaded profiles.
/// </summary>
public struct PlayerProfilePackageMessage : INetworkMessage
{
    /// <summary>
    /// Data that contains profiles in this message.
    /// </summary>
    public byte[] Data;

    /// <summary>
    /// Creates a new <see cref="PlayerProfilePackageMessage"/> instance.
    /// </summary>
    /// <param name="data">The list of profiles to send.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public PlayerProfilePackageMessage(byte[] data)
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));
        
        Data = data;
    }
    
    /// <inheritdoc cref="INetworkMessage.Read"/>
    public void Read(NetworkReader reader)
    {
        Data = reader.ReadBytes();
    }

    /// <inheritdoc cref="INetworkMessage.Write"/>
    public void Write(NetworkWriter writer)
    {
        writer.WriteBytes(Data);
    }
}