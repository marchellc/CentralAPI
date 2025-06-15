using System.Collections.Concurrent;

using NetworkLib.Pools;

namespace CentralAPI.ServerApp.Databases;

/// <summary>
/// Represents a collection which holds items.
/// </summary>
public class DatabaseCollection
{
    internal volatile byte id;
    
    internal volatile string path;
    internal volatile string type;
    
    internal volatile DatabaseTable table;
    internal volatile ConcurrentDictionary<string, DatabaseItem> items = new();

    /// <summary>
    /// Gets the ID of the collection.
    /// </summary>
    public byte Id => id;

    /// <summary>
    /// Gets the path to the collection directory.
    /// </summary>
    public string Path => path;

    /// <summary>
    /// Client-defined database item type.
    /// </summary>
    public string Type => type;
    
    /// <summary>
    /// Gets the number of items stored in the collection.
    /// </summary>
    public int Size => items.Count;
    
    /// <summary>
    /// Gets the table that this collection belongs to.
    /// </summary>
    public DatabaseTable Table => table;

    internal void ReadItems()
    {
        foreach (var file in Directory.GetFiles(path, "*.db"))
        {
            var name = System.IO.Path.GetFileNameWithoutExtension(file);
            var data = File.ReadAllBytes(file);
            var writer = NetworkDataPool.GetWriter();
            var item = new DatabaseItem();
            
            writer.Buffer.AddRange(data);

            item.writer = writer;
            item.name = name;
            item.path = file;
            
            items.TryAdd(name,  item);
        }

        var typeFilePath = System.IO.Path.Combine(path, "type.txt");

        if (File.Exists(typeFilePath))
            type = File.ReadAllText(typeFilePath);
        else
            type = string.Empty;
    }
}