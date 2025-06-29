using NetworkLib;
using NetworkLib.Interfaces;

namespace CentralAPI.SharedLib.Punishments.Warns;

/// <summary>
/// Used as a warn package download request.
/// </summary>
public struct WarnDownloadMessage : INetworkMessage
{
    /// <summary>
    /// Does nothing.
    /// </summary>
    public void Read(NetworkReader reader) { }
    
    /// <summary>
    /// Does nothing.
    /// </summary>
    public void Write(NetworkWriter writer) { }
}