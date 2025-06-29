using CentralAPI.ClientPlugin.Databases;
using CentralAPI.ClientPlugin.Network;

using CentralAPI.ClientPlugin.Punishments.Warns;

using LabExtended.Attributes;

using NetworkLib.Enums;

namespace CentralAPI.ClientPlugin.Punishments;

/// <summary>
/// Manages punishments.
/// </summary>
public static class PunishmentManager
{
    /// <summary>
    /// Gets the list of punishment directors.
    /// </summary>
    public static Dictionary<Type, PunishmentDirector> Directors { get; } = new();

    private static void OnDownload()
    {
        foreach (var director in Directors)
        {
            director.Value.IsDownloaded = false;
            
            director.Value.Download();
            director.Value.IsReady = true;
        }
    }

    private static void OnDisconnect(DisconnectReason reason)
    {
        foreach (var director in Directors)
        {
            director.Value.IsReady = false;
            director.Value.IsDownloaded = false;
            
            director.Value.Disconnect();
        }
    }
    
    [LoaderInitialize(1)]
    private static void OnInit()
    {
        Directors[typeof(WarnDirector)] = WarnDirector.Singleton;
        
        WarnDirector.Singleton.Load();
        
        DatabaseDirector.Downloaded += OnDownload;
        NetworkClient.Disconnected += OnDisconnect;
    }
}