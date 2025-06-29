using CentralAPI.ClientPlugin.Network;

namespace CentralAPI.ClientPlugin.Punishments;

/// <summary>
/// Base class for punishment directors.
/// </summary>
public abstract class PunishmentDirector
{
    /// <summary>
    /// Whether or not the director is ready.
    /// </summary>
    public bool IsReady { get; internal set; }
    
    /// <summary>
    /// Whether or not the director has downloaded it's data.
    /// </summary>
    public bool IsDownloaded { get; internal set; }

    /// <summary>
    /// Loads the director.
    /// </summary>
    public abstract void Load();

    /// <summary>
    /// Unloads the director.
    /// </summary>
    public abstract void Unload();

    /// <summary>
    /// Gets called once the database is downloaded.
    /// </summary>
    public abstract void Download();

    /// <summary>
    /// Gets called once the network gets disconnected.
    /// </summary>
    public abstract void Disconnect();

    /// <summary>
    /// Checks if the director is ready (and throws an exception if it isn't).
    /// </summary>
    /// <exception cref="Exception">The director is not ready.</exception>
    public void CheckReady()
    {
        if (NetworkClient.Scp is null)
            throw new Exception("The network is disconnected!");
        
        if (!IsReady)
            throw new Exception($"Director {GetType().Name} is not yet ready.");
        
        if (!IsDownloaded)
            throw new Exception($"Director {GetType().Name} has not downloaded it's data yet..");
    }
}