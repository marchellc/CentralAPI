using CentralAPI.ClientPlugin.Core;

using CentralAPI.ClientPlugin.Databases;
using CentralAPI.ClientPlugin.Databases.Extensions;

namespace CentralAPI.ClientPlugin.Punishments;

/// <summary>
/// Manages punishment IDs.
/// </summary>
public static class PunishmentID
{
    private static bool isInitialized = false;
    
    /// <summary>
    /// Gets the database table.
    /// </summary>
    public static DatabaseTable Table { get; private set; }

    /// <summary>
    /// Gets the database collection.
    /// </summary>
    public static DatabaseCollection<ulong> Collection { get; private set; }

    /// <summary>
    /// Gets the next punishment ID.
    /// <param name="id">The ID of the index key.</param>
    /// </summary>
    /// <returns>The assigned punishment ID.</returns>
    public static ulong GetNext(string id)
    {
        Collection.IncrementUInt64(id, 0, 1);
        return Collection.GetOrAdd(id, () => 0);
    }

    internal static void OnDownloaded()
    {
        if (!isInitialized)
        {
            Table = DatabaseDirector.GetOrAddTable(CentralPlugin.Config.PunishmentIdTableId);
            Collection = Table.GetOrAddCollection<ulong>(0);

            isInitialized = true;
        }
    }

    internal static void OnDisconnected()
    {
        if (isInitialized)
        {
            Table = null;
            Collection = null;
            
            isInitialized = false;
        }
    }
}