using CentralAPI.ClientPlugin.Core;
using CentralAPI.ClientPlugin.Databases.Requests;
using CentralAPI.ClientPlugin.Databases.Wrappers;

using CentralAPI.ClientPlugin.Network;

using LabExtended.Extensions;
using MEC;
using Utils.NonAllocLINQ;

namespace CentralAPI.ClientPlugin.Databases;

/// <summary>
/// Used to synchronize database items between client and server.
/// </summary>
public static class DatabaseDirector
{
    internal static readonly Dictionary<byte, DatabaseTable> tables = new();
    internal static readonly Dictionary<Type, object> wrappers = new();
    
    /// <summary>
    /// Whether or not the database has been downloaded yet.
    /// </summary>
    public static bool IsDownloaded { get; private set; }

    /// <summary>
    /// Contains a list of collections required by the client.
    /// </summary>
    public static Dictionary<byte, Dictionary<byte, Type>> RequiredCollections { get; } = new();
    
    /// <summary>
    /// Gets the server's personal database table.
    /// </summary>
    public static DatabaseTable? ServerTable { get; private set; }

    /// <summary>
    /// Gets the server's global database table.
    /// </summary>
    public static DatabaseTable? GlobalTable { get; private set; }
    
    /// <summary>
    /// Gets called before the database starts downloading.
    /// </summary>
    public static event Action? Downloading;

    /// <summary>
    /// Gets called when the database gets downloaded.
    /// </summary>
    public static event Action? Downloaded;

    /// <summary>
    /// Gets called when a database table is added.
    /// </summary>
    public static event Action<DatabaseTable>? Added;

    /// <summary>
    /// Gets called when a database table is dropped.
    /// </summary>
    public static event Action<DatabaseTable>? Dropped;

    /// <summary>
    /// Gets called when a database table is cleared.
    /// </summary>
    public static event Action<DatabaseTable>? Cleared; 
    
    /// <summary>
    /// Marks a collection as required by the client. This will have if invoked after the client connects.
    /// </summary>
    /// <param name="tableId">The required table ID.</param>
    /// <param name="collectionId">The required collection ID.</param>
    public static void RequireCollection<T>(byte tableId, byte collectionId)
        => RequiredCollections.GetOrAdd(tableId, () => new())[collectionId] = typeof(T);

    /// <summary>
    /// Marks a collection as required by the client (uses the server's global table ID defined via config).
    /// </summary>
    /// <param name="collectionId">The collection ID.</param>
    /// <typeparam name="T">The collection type.</typeparam>
    public static void RequireGlobalCollection<T>(byte collectionId)
    {
        if (CentralPlugin.Config.GlobalTable < 0 || CentralPlugin.Config.GlobalTable > 255)
            return;
        
        RequireCollection<T>((byte)CentralPlugin.Config.GlobalTable, collectionId);
    }
    
    /// <summary>
    /// Marks a collection as required by the client (uses the server's personal table ID defined via config).
    /// </summary>
    /// <param name="collectionId">The collection ID.</param>
    /// <typeparam name="T">The collection type.</typeparam>
    public static void RequirePersonalCollection<T>(byte collectionId)
    {
        if (CentralPlugin.Config.ServerTable < 0 || CentralPlugin.Config.ServerTable > 255)
            return;
        
        RequireCollection<T>((byte)CentralPlugin.Config.ServerTable, collectionId);
    }

    /// <summary>
    /// Attempts to find a table by an ID.
    /// </summary>
    /// <param name="tableId">The ID of the table.</param>
    /// <param name="table">The found table.</param>
    /// <returns>true if the table was found</returns>
    public static bool TryGetTable(byte tableId, out DatabaseTable table)
        => tables.TryGetValue(tableId, out table);

    /// <summary>
    /// Finds a table by an ID.
    /// </summary>
    /// <param name="tableId">The ID of the table.</param>
    /// <returns>The found table instance.</returns>
    /// <exception cref="Exception">Table with that ID does not exist.</exception>
    public static DatabaseTable GetTable(byte tableId)
    {
        if (!TryGetTable(tableId, out var table))
            throw new Exception($"Table {tableId} was not found.");

        return table;
    }

    /// <summary>
    /// Finds or adds a new table.
    /// </summary>
    /// <param name="tableId">The ID of the table.</param>
    /// <returns>The found or added table.</returns>
    public static DatabaseTable GetOrAddTable(byte tableId)
    {
        if (TryGetTable(tableId, out var table))
            return table;

        table = new();
        table.Id = tableId;
        
        Added?.InvokeSafe(table);
        
        tables.Add(tableId, table);

        AddTableRequest.SendRequest(tableId);
        return table;
    }

    /// <summary>
    /// Drops a table.
    /// </summary>
    /// <param name="tableId">The ID of the table to drop.</param>
    public static void DropTable(byte tableId)
    {
        if (TryGetTable(tableId, out var table))
        {
            Dropped?.InvokeSafe(table);
            
            tables.Remove(tableId);
            
            table.InternalDestroy(true);
            
            ClearTableRequest.SendRequest(table.Id, true);
        }
    }

    /// <summary>
    /// Clears a table (drops all collections in a table).
    /// </summary>
    /// <param name="tableId">The iD of the table to clear.</param>
    public static void ClearTable(byte tableId)
    {
        if (TryGetTable(tableId, out var table)
            && table.Size > 0)
        {
            table.InternalClear();
            
            Cleared?.InvokeSafe(table);
            
            ClearTableRequest.SendRequest(table.Id, false);
        }
    }

    /// <summary>
    /// Registers a database wrapper.
    /// </summary>
    /// <param name="wrapper">The wrapper instance to register.</param>
    /// <typeparam name="T">The type that is wrapped around by the wrapper.</typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    public static void RegisterWrapper<T>(DatabaseWrapper<T> wrapper)
    {
        if (wrapper is null)
            throw new ArgumentNullException(nameof(wrapper));

        wrappers[typeof(T)] = wrapper;
    }
    
    /// <summary>
    /// Attempts to find a wrapper for a given type.
    /// </summary>
    /// <param name="wrapper">The resolved wrapper instance.</param>
    /// <typeparam name="T">The wrapped type.</typeparam>
    /// <returns>true if the wrapper was found</returns>
    public static bool TryGetWrapper<T>(out DatabaseWrapper<T> wrapper)
    {
        if (wrappers.TryGetValue(typeof(T), out var reference))
        {
            wrapper = (DatabaseWrapper<T>)reference;
            return true;
        }

        if (typeof(T).IsArray)
        {
            var elementType = typeof(T).GetElementType();

            if (wrappers.TryGetValue(elementType, out reference))
            {
                wrapper =
                    Activator.CreateInstance(typeof(DatabaseArray<>).MakeGenericType(elementType), reference) as
                        DatabaseWrapper<T>;

                wrappers[typeof(T)] = wrapper;
                return true;
            }
        }
        else if (typeof(T).GetGenericTypeDefinition() == typeof(List<>))
        {
            var elementType = typeof(T).GetGenericArguments()[0];
            
            if (wrappers.TryGetValue(elementType, out reference))
            {
                wrapper =
                    Activator.CreateInstance(typeof(DatabaseList<>).MakeGenericType(elementType), reference) as
                        DatabaseWrapper<T>;

                wrappers[typeof(T)] = wrapper;
                return true;
            }
        }
        else if (typeof(T).GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            var keyType = typeof(T).GetGenericArguments()[0];
            var valueType = typeof(T).GetGenericArguments()[1];

            if (wrappers.TryGetValue(keyType, out var keyReference)
                && wrappers.TryGetValue(valueType, out var valueReference))
            {
                wrapper =
                    Activator.CreateInstance(typeof(DatabaseDictionary<,>).MakeGenericType(keyType, valueType), keyReference, valueReference) as
                        DatabaseWrapper<T>;

                wrappers[typeof(T)] = wrapper;
                return true;
            }
        }

        wrapper = null;
        return false;
    }

    private static void RegisterWrappers()
    {
        RegisterWrapper(new DatabaseBool());
        
        RegisterWrapper(new DatabaseChar());
        RegisterWrapper(new DatabaseString());
        
        RegisterWrapper(new DatabaseIpAddress());
        RegisterWrapper(new DatabaseIpEndPoint());
        
        RegisterWrapper(new DatabaseDateTime());
        RegisterWrapper(new DatabaseTimeSpan());
        
        RegisterWrapper(new DatabaseByte());
        RegisterWrapper(new DatabaseSByte());
        
        RegisterWrapper(new DatabaseInt16());
        RegisterWrapper(new DatabaseUInt16());
        
        RegisterWrapper(new DatabaseInt32());
        RegisterWrapper(new DatabaseUInt32());
        
        RegisterWrapper(new DatabaseInt64());
        RegisterWrapper(new DatabaseUInt64());
        
        RegisterWrapper(new DatabaseDouble());
        RegisterWrapper(new DatabaseFloat());
    }
    
    private static void OnReady()
    {
        Timing.CallDelayed(5f, () =>
        {
            DatabaseRequests.OnReady();

            if (RequiredCollections.Any(x => x.Value?.Count > 0))
                EnsureRequest.SendRequest();

            DownloadRequest.SendRequest();
        });
    }
    
    private static void OnDisconnected()
    {
        IsDownloaded = false;
        
        foreach (var table in tables)
            table.Value.InternalDestroy();
        
        tables.Clear();
    }

    internal static void OnDownloaded()
    {
        if (IsDownloaded)
            return;
        
        IsDownloaded = true;

        if (CentralPlugin.Config.ServerTable > -1 && CentralPlugin.Config.ServerTable < byte.MaxValue)
            ServerTable = GetOrAddTable((byte)CentralPlugin.Config.ServerTable);
        
        if (CentralPlugin.Config.GlobalTable > -1 && CentralPlugin.Config.GlobalTable < byte.MaxValue)
            GlobalTable = GetOrAddTable((byte)CentralPlugin.Config.GlobalTable);
        
        Downloaded?.InvokeSafe();
    }

    internal static void OnDownloading()
    {
        if (IsDownloaded)
            return;
        
        Downloading?.InvokeSafe();
    }

    internal static void OnAdded(DatabaseTable table)
    {
        Added?.InvokeSafe(table);
    }

    internal static void OnDropped(DatabaseTable table)
    {
        Dropped?.InvokeSafe(table);
    }

    internal static void Init()
    {
        NetworkClient.Ready += OnReady;
        NetworkClient.Destroyed += OnDisconnected;
        
        RegisterWrappers();
    }
}