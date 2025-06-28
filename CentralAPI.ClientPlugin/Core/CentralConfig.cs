using System.ComponentModel;

using CentralAPI.ClientPlugin.Core.Configs;

namespace CentralAPI.ClientPlugin.Core;

/// <summary>
/// Plugin configuration file.
/// </summary>
public class CentralConfig
{
    [Description("Network client configuration.")]
    public NetworkConfig Network { get; set; } = new();
    
    [Description("Database client configuration.")]
    public DatabaseConfig Database { get; set; } = new();
    
    [Description("ID of the table that will contain punishment IDs. This should be a fully custom table as collection IDs are NOT customizable.")]
    public byte PunishmentIdTableId { get; set; }
    
    [Description("Warn punishments configuration.")]
    public PunishmentsConfig Warns { get; set; } = new();
}