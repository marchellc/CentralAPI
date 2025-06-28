using System.ComponentModel;

namespace CentralAPI.ClientPlugin.Core.Configs;

/// <summary>
/// Database configuration.
/// </summary>
public class DatabaseConfig
{
    [Description("Sets the server's personal database table ID (values below zero disable this, max. 255).")]
    public int ServerTable { get; set; } = -1;
    
    [Description("Sets the server's global database table ID (values below zero disable this, max. 255).")]
    public int GlobalTable { get; set; } = -1;
}