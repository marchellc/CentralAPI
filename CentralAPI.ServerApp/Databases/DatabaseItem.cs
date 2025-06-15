using NetworkLib;

namespace CentralAPI.ServerApp.Databases;

/// <summary>
/// Represents a single database item.
/// </summary>
public class DatabaseItem
{
    internal volatile NetworkWriter writer;
    
    internal volatile string name;
    internal volatile string path;

    /// <summary>
    /// Gets the name of the item.
    /// </summary>
    public string Name => name;
    
    /// <summary>
    /// Gets the path to the item's file.
    /// </summary>
    public string Path => path;
    
    /// <summary>
    /// Gets the item's data.
    /// </summary>
    public NetworkWriter Writer => writer;
}