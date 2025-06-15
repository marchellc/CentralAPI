using LabExtended.Core;

using NetworkLib;
using NetworkClient = CentralAPI.ClientPlugin.Network.NetworkClient;

namespace CentralAPI.ClientPlugin.Databases.Requests;

internal static class ClearCollectionRequest
{
    private static ushort clearCollectionCode;
    
    internal static void Init()
    {
        clearCollectionCode = NetworkClient.Scp.GetRequestType("Database.ClearCollection");
        
        NetworkClient.scp.HandleRequest("Database.ClearCollection", HandleRequest);
    }

    internal static void SendRequest(byte tableId, byte collectionId, bool drop)
    {
        ApiLog.Debug("Database Director", $"&3[ClearCollectionRequest]&r SENDING: TableId={tableId}; CollectionId={collectionId}; Drop={drop}");
        
        NetworkClient.Scp.Request(clearCollectionCode, writer =>
        {
            writer.WriteByte(tableId);
            writer.WriteByte(collectionId);
            writer.WriteBool(drop);
        }, HandleResponse);
    }

    private static void HandleResponse(NetworkReader reader)
    {
        var result = reader.ReadByte();

        switch (result)
        {
            case 0:
                ApiLog.Debug("Database Director", $"&3[ClearCollectionRequest]&r &2OK&r");
                break;
            
            case 1:
                ApiLog.Debug("Database Director", $"&3[ClearCollectionRequest]&r &1Unknown Table&r");
                break;
            
            case 2:
                ApiLog.Debug("Database Director", $"&3[ClearCollectionRequest]&r &1Unknown Collection&r");
                break;
            
            case 3:
                ApiLog.Debug("Database Director", $"&3[ClearCollectionRequest]&r &1{reader.ReadString()}&r");
                break;
        }
    }
    
    private static void HandleRequest(NetworkReader reader, NetworkWriter writer)
    {
        var tableId = reader.ReadByte();
        var collectionId = reader.ReadByte();
        var drop = reader.ReadBool();
        
        ApiLog.Debug("Database Director", $"&3[ClearCollectionRequest]&r RECEIVED: TableId={tableId}; CollectionId={collectionId}; Drop={drop}");

        if (!DatabaseDirector.TryGetTable(tableId, out var table))
        {
            ApiLog.Warn("Database Director", $"&3[ClearCollectionRequest]&r Unknown table: {tableId}");
            return;
        }

        if (!table.collections.TryGetValue(collectionId, out var collection))
        {
            ApiLog.Warn("Database Director", $"&3[ClearCollectionRequest]&r Unknown collection: {collectionId}");
            return;
        }

        if (drop)
        {
            table.InternalDropCollection(collection);
            
            ApiLog.Debug("Database Director", $"&3[ClearCollectionRequest]&r Dropped {collection.GetLogPath()}");
        }
        else
        {
            table.InternalClearCollection(collection);
            
            ApiLog.Debug("Database Director", $"&3[ClearCollectionRequest]&r Cleared {collection.GetLogPath()}");
        }
    }
}