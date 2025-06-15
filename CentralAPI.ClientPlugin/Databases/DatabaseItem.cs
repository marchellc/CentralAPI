using CentralAPI.ClientPlugin.Databases.Internal;

namespace CentralAPI.ClientPlugin.Databases;

/// <summary>
/// Represents an item stored in a collection.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public class DatabaseItem<T> : DatabaseItemBase
{
    internal T value;
    
    /// <summary>
    /// Gets or sets the value of the item.
    /// </summary>
    public T? Value
    {
        get => value;
        set
        {
            this.value = value;
            
            Collection.OnUpdated(this);
        }
    }
    
    /// <summary>
    /// Gets the item's collection.
    /// </summary>
    public DatabaseCollection<T> Collection { get; internal set; }
}