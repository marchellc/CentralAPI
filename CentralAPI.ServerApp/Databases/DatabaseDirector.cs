using System.Collections.Concurrent;

using CentralAPI.ServerApp.Server;

using NetworkLib;

namespace CentralAPI.ServerApp.Databases;

/// <summary>
/// Manages databases and their synchronization.
/// </summary>
public static class DatabaseDirector
{
    internal static volatile string path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "tables");
    internal static volatile ConcurrentDictionary<byte, DatabaseTable> tables = new();

    /// <summary>
    /// Gets the path to the base directory.
    /// </summary>
    public static string Path => path;
    
    /// <summary>
    /// Gets the dictionary that contains all loaded tables.
    /// </summary>
    public static ConcurrentDictionary<byte, DatabaseTable> Tables => tables;

    internal static void SendToOthers(ScpInstance ignoreReceiver, ushort requestCode, Action<NetworkWriter> writer)
    {
        foreach (var instance in ScpManager.PortToServer)
        {
            if (ignoreReceiver != null && ignoreReceiver.Port == instance.Key)
                continue;
            
            instance.Value.Request(requestCode, writer);
        }
    }

    internal static void Init()
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        foreach (var directory in Directory.GetDirectories(path))
        {
            var table = new DatabaseTable();

            table.id = byte.Parse(System.IO.Path.GetFileName(directory));
            table.path = directory;

            tables.TryAdd(table.id, table);
            
            table.ReadCollections();
        }
    }
}