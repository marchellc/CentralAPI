using System.ComponentModel;

namespace CentralAPI.ClientPlugin.Core.Configs;

/// <summary>
/// Network configuration.
/// </summary>
public class NetworkConfig
{
    [Description("The alias that will be used on the server.")]
    public string ServerAlias { get; set; } = string.Empty;
    
    [Description("Server IP.")] 
    public string ServerAddress { get; set; } = "127.0.0.1";
    
    [Description("Server Port.")] 
    public ushort ServerPort { get; set; } = 8888;

    [Description("Client's internal buffer size.")]
    public int BufferSize { get; set; } = ushort.MaxValue;

    [Description("How many seconds to wait before sending a heartbeat.")]
    public int HeartbeatSeconds { get; set; } = 10;
    
    [Description("Maximum amount of connection attempts (includes reconnection).")]
    public int MaxConnectAttempts { get; set; } = 5;

    [Description("Whether or not the client should attempt to automatically reconnect.")]
    public bool AllowReconnection { get; set; } = true;
}