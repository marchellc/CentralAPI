using NetworkLib;

namespace CentralAPI.ClientPlugin.Databases.Internal;

/// <summary>
/// Base class for database items.
/// </summary>
public class DatabaseItemBase
{
    internal DatabaseCollectionBase collection;
    internal NetworkReader reader;
    internal Type type;

    internal bool hasRead;
    
    /// <summary>
    /// Gets the name of the item.
    /// </summary>
    public string Name { get; internal set; }

    internal string GetLogPath()
    {
        var str = string.Empty;

        if (collection != null)
        {
            if (collection.Table != null)
                str += $"{collection.Table.Id}/";
            else
                str += "NullTable/";

            str += $"{collection.Id}/";
        }
        else
        {
            str += "NullTable/NullCollection/";
        }

        if (Name?.Length > 0)
            str += Name;
        else
            str += "NullName";
        
        return str;
    }
}