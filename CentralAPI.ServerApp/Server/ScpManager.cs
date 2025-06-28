using System.Collections.Concurrent;

using CommonLib;

using NetworkLib;
using NetworkLib.Interfaces;
using NetworkServer = CentralAPI.ServerApp.Network.NetworkServer;

namespace CentralAPI.ServerApp.Server;

/// <summary>
/// Manages connected SCP servers.
/// </summary>
public static class ScpManager
{
    /// <summary>
    /// Gets called when a connected server is identified.
    /// </summary>
    public static event Action<ScpInstance>? Identified;

    /// <summary>
    /// Gets called when a connected server disconnects.
    /// </summary>
    public static event Action<ScpInstance>? Disconnected;

    /// <summary>
    /// Contains a list of identified SCP servers, keyed by their server list port.
    /// </summary>
    public static volatile ConcurrentDictionary<ushort, ScpInstance> PortToServer = new();
    
    /// <summary>
    /// Contains a list of identified and unidentified SCP servers, keyed by their connection.
    /// </summary>
    public static volatile ConcurrentDictionary<NetworkConnection, ScpInstance> ConnectionToServer = new();

    /// <summary>
    /// Sends a message to all connected servers that match a predicate.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="predicate">The server filtering predicate.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void SendToWhere(this INetworkMessage message, Predicate<ScpInstance> predicate)
    {
        if (message is null)
            throw new ArgumentNullException(nameof(message));
        
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        foreach (var scpInstance in ConnectionToServer)
        {
            if (!predicate(scpInstance.Value))
                continue;

            scpInstance.Value.Send(message);
        }
    }

    /// <summary>
    /// Sends a message to all connected servers.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="onlyVerified">Whether or not to send the message only to servers that have verified.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void SendToAll(this INetworkMessage message, bool onlyVerified = false)
    {
        if (message is null)
            throw new ArgumentNullException(nameof(message));

        if (onlyVerified)
        {
            foreach (var scpInstance in PortToServer)
            {
                scpInstance.Value.Send(message);
            }
        }
        else
        {
            foreach (var scpInstance in ConnectionToServer)
            {
                scpInstance.Value.Send(message);
            }
        }
    }

    private static void OnSynchronized(NetworkConnection connection)
    {
        Task.Run(async () =>
        {
            await Task.Delay(5000);
            
            CommonLog.Debug("Scp Manager", $"Connection '{connection.Peer.Client.RemoteEndPoint}' has synchronized.");

            connection.AddComponent<ScpInstance>();
        });
    }

    internal static void SetIdentified(ScpInstance scpInstance)
    {
        CommonLog.Info("Scp Manager", $"Connection '{scpInstance.Connection.Peer.Client.RemoteEndPoint}' has identified itself as server at port {scpInstance.Port} ({scpInstance.Alias}; {scpInstance.Name})");
        
        PortToServer.TryRemove(scpInstance.Port, out _);
        PortToServer.TryAdd(scpInstance.Port, scpInstance);
        
        Identified?.Invoke(scpInstance);
    }

    internal static void SetTerminated(ScpInstance scpInstance)
    {
        if (scpInstance.Port != 0)
            PortToServer.TryRemove(scpInstance.Port, out _);
        
        if (scpInstance.Connection != null)
            ConnectionToServer.TryRemove(scpInstance.Connection, out _);
        
        Disconnected?.Invoke(scpInstance);
    }
    
    internal static void Init()
    {
        if (NetworkServer.Server != null)
        {
            NetworkServer.Server.OnSynchronized += OnSynchronized;
        }
        else
        {
            CommonLog.Warn("Scp Manager", "Network Server is null!");
        }
    }
}