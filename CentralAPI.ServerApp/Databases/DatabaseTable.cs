using System.Collections.Concurrent;

namespace CentralAPI.ServerApp.Databases;

/// <summary>
/// Represents a database table which holds collections.
/// </summary>
public class DatabaseTable
{
    internal volatile byte id;
    internal volatile string path;
    
    internal volatile ConcurrentDictionary<byte, DatabaseCollection> collections = new();

    /// <summary>
    /// Gets the ID of the table.
    /// </summary>
    public byte Id => id;

    /// <summary>
    /// Gets the path to the table directory.
    /// </summary>
    public string Path => path;

    /// <summary>
    /// Gets the number of collections stored in the table.
    /// </summary>
    public int Size => collections.Count;

    internal void ReadCollections()
    {
        foreach (var directory in Directory.GetDirectories(path))
        {
            var collection = new DatabaseCollection();
            
            collection.id = byte.Parse(System.IO.Path.GetFileName(directory));
            collection.path = directory;
            collection.table = this;

            collections.TryAdd(collection.id, collection);
            
            collection.ReadItems();
        }
    }
}