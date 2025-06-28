using CentralAPI.ClientPlugin.Core.Configs;
using CentralAPI.ClientPlugin.Databases;
using CentralAPI.ClientPlugin.Network;
using CentralAPI.ClientPlugin.PlayerProfiles;

using LabApi.Loader.Features.Plugins;

namespace CentralAPI.ClientPlugin.Core;

/// <summary>
/// The main class of the client plugin.
/// </summary>
public class CentralPlugin : Plugin<CentralConfig>
{
    private static volatile CentralPlugin plugin;
    private static volatile CentralConfig config;
    
    /// <summary>
    /// Gets the active plugin instance.
    /// </summary>
    public static CentralPlugin Plugin => plugin;
    
    /// <summary>
    /// Gets the active config instance.
    /// </summary>
    public new static CentralConfig Config => config;

    /// <summary>
    /// Gets the active network config.
    /// </summary>
    public static NetworkConfig Network => Config.Network;
    
    /// <summary>
    /// Gets the active database config.
    /// </summary>
    public static DatabaseConfig Database => Config.Database;
    
    /// <summary>
    /// Gets the active warns config.
    /// </summary>
    public static PunishmentsConfig Warns => Config.Warns;
    
    /// <inheritdoc cref="Plugin.Author"/>
    public override string Author { get; } = "marchellcx";
    
    /// <inheritdoc cref="Plugin.Name"/>
    public override string Name { get; } = "CentralAPI";
    
    /// <inheritdoc cref="Plugin.Description"/>
    public override string Description { get; } = "A client for the CentralAPI framework.";
    
    /// <inheritdoc cref="Plugin.Version"/>
    public override Version Version { get; } = new(1, 0, 0);

    /// <inheritdoc cref="Plugin.RequiredApiVersion"/>
    public override Version RequiredApiVersion { get; } = null;
    
    /// <inheritdoc cref="Plugin.Enable"/>
    public override void Enable()
    {
        plugin = this;
        config = base.Config;
        
        PlayerProfileManager.Init();
        DatabaseDirector.Init();
        NetworkClient.Init();
    }

    /// <inheritdoc cref="Plugin.Disable"/>
    public override void Disable()
    {

    }
}