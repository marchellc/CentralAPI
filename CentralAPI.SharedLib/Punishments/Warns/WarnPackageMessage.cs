using NetworkLib;
using NetworkLib.Interfaces;

namespace CentralAPI.SharedLib.Punishments.Warns;

/// <summary>
/// Sends all saved warns to connected servers.
/// </summary>
public struct WarnPackageMessage : INetworkMessage
{
    /// <summary>
    /// The warn package data.
    /// </summary>
    public byte[] Data;

    /// <summary>
    /// Creates a new <see cref="WarnPackageMessage"/> instance.
    /// </summary>
    /// <param name="data">The warn package data.</param>
    public WarnPackageMessage(byte[] data)
    {
        Data = data;
    }

    /// <inheritdoc cref="INetworkMessage.Read"/>>
    public void Read(NetworkReader reader)
    {
        Data = reader.ReadBytes();
    }

    /// <inheritdoc cref="INetworkMessage.Write"/>>
    public void Write(NetworkWriter writer)
    {
        writer.WriteBytes(Data);
    }
}