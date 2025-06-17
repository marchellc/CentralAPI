namespace CentralAPI.ClientPlugin.Databases.Internal;

/// <summary>
/// Base class for database collections.
/// </summary>
public class DatabaseCollectionBase
{
    /// <summary>
    /// Gets the ID of the collection.
    /// </summary>
    public byte Id { get; internal set; }
    
    /// <summary>
    /// Gets the number of items stored in the collection.
    /// </summary>
    public virtual int Size { get; }
    
    /// <summary>
    /// Gets the type of item in this collection.
    /// </summary>
    public Type Type { get; internal set; }
    
    /// <summary>
    /// Gets the parent database table.
    /// </summary>
    public DatabaseTable Table { get; internal set; }
    
    internal virtual Type ItemType { get; }

    internal virtual bool InternalTryGetString(string name, out string value)
    {
        value = null;
        return false;
    }

    internal virtual bool InternalTryGet(string name, out DatabaseItemBase item)
    {
        item = null;
        return false;
    }

    internal virtual void InternalUpdate(DatabaseItemBase item)
    {
        
    }

    internal virtual void InternalClear()
    {
        
    }

    internal virtual void InternalDestroy(bool isDrop = false)
    {
        
    }
    
    internal virtual void InternalInit()
    {
        
    }

    internal virtual void InternalAddItem(DatabaseItemBase item)
    {
        
    }

    internal virtual void InternalRemoveItem(DatabaseItemBase item)
    {
        
    }

    internal string GetLogPath()
    {
        var str = string.Empty;

        if (Table != null)
            str += $"{Table.Id}/";
        else
            str += "NullTable/";

        str += $"{Id}";
        return str;
    }
}