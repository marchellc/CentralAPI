using CentralAPI.ServerApp.Core.Configs;
using CentralAPI.ServerApp.Core.Loader;

using CentralAPI.SharedLib.Requests; 

using CommonLib;

using NetworkLib;
using NetworkLib.Entities; 

using Newtonsoft.Json;

namespace CentralAPI.ServerApp.Network;

/// <summary>
/// Manages TCP server connection.
/// </summary>
public static class NetworkServer
{
    /// <summary>
    /// Represents the server's configuration file.
    /// </summary>
    public class NetworkConfig
    {
        /// <summary>
        /// Receive buffer size.
        /// </summary>
        [JsonProperty("buffer_size")]
        public int BufferSize { get; set; } = ushort.MaxValue;

        /// <summary>
        /// How long can a delay between a heartbeat be.
        /// </summary>
        [JsonProperty("heartbeat_seconds")]
        public int HeartbeatSeconds { get; set; } = 10;

        [JsonProperty("server_port")]
        public ushort Port { get; set; } = 0;
    }

    private static volatile NetworkConfig config;
    private static volatile NetworkLib.NetworkServer server;
    
    /// <summary>
    /// Gets the network server config.
    /// </summary>
    public static NetworkConfig Config => config;

    /// <summary>
    /// Gets the network server instance.
    /// </summary>
    public static NetworkLib.NetworkServer Server => server;
    
    internal static void Init()
    {
        config = ConfigLoader.Load("network", new NetworkConfig());
        
        server = new();
        
        server.BufferSize = config.BufferSize;
        server.HeartbeatSeconds = config.HeartbeatSeconds;
        
        server.AddComponent<NetworkEntityManager>();
        
        Loader.Updated += Update;
        
        NetworkLibrary.CollectMessageTypes();
        
        NetworkLibrary.MessageTypes = NetworkLibrary.MessageTypes.Concat([
            typeof(RequestMessage),
            typeof(ResponseMessage)
        ]).ToArray();
        
        CommonLog.Info("Network Server", $"Starting the server on port '{config.Port}' ..");

        try
        {
            server.Start(config.Port);
            
            CommonLog.Info("Network Server", "Server started!");
        }
        catch (Exception ex)
        {
            Loader.Report(ex, LoaderExceptionSeverity.High, "NetworkServer", "ServerLoad", null);
        }
        
        Loader.Exiting += Quit;
    }

    private static void Quit()
    {
        if (config != null)
            ConfigLoader.Write("network", config);

        if (server != null)
        {
            server.Stop();
            server = null;
        }
    }
    
    private static void Update()
    {
        if (server != null)
        {
            server.Update();
        }
    }
}