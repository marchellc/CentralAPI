using NetworkLib;
using NetworkLib.Interfaces;

namespace CentralAPI.SharedLib.PlayerProfiles;

/// <summary>
/// Message used to request player profile package.
/// </summary>
public struct PlayerProfileDownloadMessage : INetworkMessage
{
    /// <summary>
    /// Does nothing.
    /// </summary>
    /// <param name="reader"></param>
    public void Read(NetworkReader reader) { }

    /// <summary>
    /// Does nothing.
    /// </summary>
    /// <param name="writer"></param>
    public void Write(NetworkWriter writer) { }
}