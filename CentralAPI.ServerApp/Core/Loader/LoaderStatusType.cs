namespace CentralAPI.ServerApp.Core.Loader;

/// <summary>
/// Description of the loader status.
/// </summary>
public enum LoaderStatusType
{
    /// <summary>
    /// The loader is starting.
    /// </summary>
    Starting,
    
    /// <summary>
    /// The loader is running.
    /// </summary>
    Running,
    
    /// <summary>
    /// The loader is exiting.
    /// </summary>
    Stopping
}