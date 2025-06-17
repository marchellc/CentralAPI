using CentralAPI.ClientPlugin.Databases;
using CentralAPI.ClientPlugin.Network;

using LabApi.Loader.Features.Plugins;
using LabExtended.Commands;

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
        
        DatabaseDirector.Init();
        NetworkClient.Init();
    }

    /// <inheritdoc cref="Plugin.Disable"/>
    public override void Disable()
    {

    }
}