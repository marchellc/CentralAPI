using CentralAPI.ClientPlugin.Databases.Internal;

using LabExtended.Core;

using NetworkLib;
using NetworkClient = CentralAPI.ClientPlugin.Network.NetworkClient;

namespace CentralAPI.ClientPlugin.Databases.Requests;

internal static class AddCollectionRequest
{
    private static ushort addCollectionCode;

    internal static void Init()
    {
        addCollectionCode = NetworkClient.scp.GetRequestType("Database.AddCollection");
        
        NetworkClient.Scp.HandleRequest("Database.AddCollection", HandleRequest);
    }

    internal static void SendRequest(byte tableId, byte collectionId, string collectionType)
    {
        NetworkClient.Scp.Request(addCollectionCode, writer =>
        {
            writer.WriteByte(tableId);
            writer.WriteByte(collectionId);
            
            writer.WriteString(collectionType);
        }, HandleResponse);
    }
    
    private static void HandleResponse(NetworkReader reader)
    {
        if (reader.ReadByte() != 0)
            ApiLog.Debug("Database Director", $"&3[AddCollectionRequest]&r &1{reader.ReadString()}&r");
        else
            ApiLog.Debug("Database Director", $"&3[AddCollectionRequest]&r &2OK&r");
    }

    private static void HandleRequest(NetworkReader reader, NetworkWriter writer)
    {
        var tableId = reader.ReadByte();
        
        var collectionId = reader.ReadByte();
        var collectionType = reader.ReadString();
        
        ApiLog.Debug("Database Director", $"&3[AddCollectionRequest]&r RECEIVED: TableId={tableId}; CollectionId={collectionId}; CollectionType={collectionType}");

        if (!DatabaseDirector.TryGetTable(tableId, out var table))
        {
            table = new();
            table.Id = tableId;
            
            DatabaseDirector.tables.Add(tableId, table);
            DatabaseDirector.OnAdded(table);
            
            ApiLog.Debug("Database Director", $"&3[AddCollectionRequest]&r Added table {tableId}");
        }
        else
        {
            ApiLog.Debug("Database Director", $"&3[AddCollectionRequest]&r Found table {tableId}");
        }

        if (!table.collections.TryGetValue(collectionId, out var collection))
        {
            var type = Type.GetType(collectionType, true);
            
            collection = Activator.CreateInstance(typeof(DatabaseCollection<>).MakeGenericType(type)) as DatabaseCollectionBase;
            
            collection.Table = table;
                
            collection.Id = collectionId;
            collection.Type = type;
            
            table.collections.Add(collectionId, collection);
            
            collection.InternalInit();
            
            table.OnAdded(collection);
            
            ApiLog.Debug("Database Director", $"&3[AddCollectionRequest]&r Added collection {collectionId} ({collection.Type.FullName})");
        }
        else
        {
            ApiLog.Debug("Database Director", $"&3[AddCollectionRequest]&r Found collection {collectionId}");
        }
    }
}