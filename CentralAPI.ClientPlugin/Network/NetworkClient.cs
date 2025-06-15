using System.Net;

using CentralAPI.ClientPlugin.Core;
using CentralAPI.ClientPlugin.Databases;
using CentralAPI.ClientPlugin.Logging;

using CentralAPI.SharedLib.Requests;

using CommonLib;

using LabExtended.Core;
using LabExtended.Extensions;

using LabExtended.Utilities;
using LabExtended.Utilities.Update;

using NetworkLib; 
using NetworkLib.Enums; 

namespace CentralAPI.ClientPlugin.Network;

/// <summary>
/// Starts and manages the TCP network client.
/// </summary>
public static class NetworkClient
{
    private static volatile NetworkLib.NetworkClient client;

    internal static volatile ScpInstance scp;
    
    /// <summary>
    /// Gets the current client instance.
    /// </summary>
    public static NetworkLib.NetworkClient Client => client;
    
    /// <summary>
    /// Gets the active connection.
    /// </summary>
    public static NetworkConnection? Connection => client?.Connection;

    /// <summary>
    /// Gets the active SCP client.
    /// </summary>
    public static ScpInstance? Scp => scp;

    /// <summary>
    /// Gets called when the client starts.
    /// </summary>
    public static event Action? Started;

    /// <summary>
    /// Gets called when the client stops.
    /// </summary>
    public static event Action? Stopped;

    /// <summary>
    /// Gets called when the client connects.
    /// </summary>
    public static event Action? Connected;
    
    /// <summary>
    /// Gets called when the client disconnects.
    /// </summary>
    public static event Action<DisconnectReason>? Disconnected;

    /// <summary>
    /// Gets called when the client receives the synchronization packet.
    /// </summary>
    public static event Action? Synchronized;

    /// <summary>
    /// Gets called when the client is ready.
    /// </summary>
    public static event Action? Ready;

    /// <summary>
    /// Gets called when the client is destroyed.
    /// </summary>
    public static event Action? Destroyed;
    
    internal static void Init()
    {
        if (!IPAddress.TryParse(CentralPlugin.Config.ServerAddress, out var serverIp))
        {
            ApiLog.Warn("Network Client", $"Could not parse server IP: &3{CentralPlugin.Config.ServerAddress}&r");
            return;
        }
        
        LogDisplay.Init();
        
        CommonLibrary.Initialize(StartupArgs.Args);
        CommonLog.IsDebugEnabled = ApiLog.CheckDebug("CommonLibrary");
        
        client = new(CentralPlugin.Config.BufferSize);
        
        client.AllowReconnection = CentralPlugin.Config.AllowReconnection;
        client.HeartbeatSeconds =  CentralPlugin.Config.HeartbeatSeconds;
        client.MaxConnectionAttempts = CentralPlugin.Config.MaxConnectAttempts;
        
        client.OnStarted += OnStarted;
        client.OnStopped += OnStopped;
        
        client.OnConnected += OnConnected;
        client.OnDisconnected += OnDisconnected;
        
        client.OnSynchronized += OnSynchronized;
        client.OnFailed += OnFailed;

        Task.Run(NetworkLibrary.CollectMessageTypes).ContinueWithOnMain(_ =>
        {
            NetworkLibrary.MessageTypes = NetworkLibrary.MessageTypes.Concat([
                typeof(RequestMessage),
                typeof(ResponseMessage)
            ]).ToArray();
            
            PlayerUpdateHelper.OnUpdate += Update;

            ApiLog.Info("Network Client", $"Connecting to: &1{serverIp}:{CentralPlugin.Config.ServerPort}&r ..");

            client.Connect(new(serverIp, CentralPlugin.Config.ServerPort));
        });
    }
    
    internal static void OnReady()
        => Ready?.InvokeSafe();

    internal static void OnDestroyed()
        => Destroyed?.InvokeSafe();

    private static void OnFailed()
    {
        ApiLog.Warn("Network Client", $"Could not connect to &3{CentralPlugin.Config.ServerAddress}:{CentralPlugin.Config.ServerPort}&r!");
    }

    private static void OnSynchronized()
    {
        Connection.AddComponent<ScpInstance>();
        Synchronized?.InvokeSafe();
    }

    private static void OnDisconnected(DisconnectReason reason)
    {
        Disconnected?.InvokeSafe(reason);
    }

    private static void OnConnected()
    {
        Connected?.InvokeSafe();
    }

    private static void OnStarted()
    {
        Started?.InvokeSafe();
    }

    private static void OnStopped()
    {
        Stopped?.InvokeSafe();
    }

    private static void Update()
    {
        try
        {
            client?.Update();
        }
        catch (Exception ex)
        {
            ApiLog.Error("Network Client", $"Could not update the client:\n{ex}");
        }
    }
}