using LabExtended.Core;

using NetworkLib;
using NetworkClient = CentralAPI.ClientPlugin.Network.NetworkClient;

namespace CentralAPI.ClientPlugin.Databases.Requests;

internal static class RemoveItemRequest
{
    private static ushort removeItemCode;
    
    internal static void Init()
    {
        removeItemCode = NetworkClient.Scp.GetRequestType("Database.RemoveItem");
        
        NetworkClient.scp.HandleRequest("Database.RemoveItem", HandleRequest);
    }

    internal static void SendRequest(byte tableId, byte collectionId, Action<NetworkWriter> writeItems)
    {
        ApiLog.Debug("Database Director", $"&3[RemoveItemRequest]&r SENDING: TableId={tableId}; CollectionId={collectionId}");
        
        NetworkClient.scp.Request(removeItemCode, writer =>
        {
            writer.WriteByte(tableId);
            writer.WriteByte(collectionId);
            
            writeItems(writer);
        });
    }

    private static void HandleRequest(NetworkReader reader, NetworkWriter writer)
    {
        var tableId = reader.ReadByte();
        var collectionId = reader.ReadByte();

        var itemCount = reader.ReadByte();
        
        ApiLog.Debug("Database Director", $"&3[RemoveItemRequest]&r RECEIVED: TableId={tableId}; CollectionId={collectionId}; ItemCount={itemCount}");

        if (!DatabaseDirector.TryGetTable(tableId, out var table))
        {
            ApiLog.Warn("Database Director", $"&3[RemoveItemRequest]&r Unknown table: {tableId}");
            return;
        }

        if (!table.collections.TryGetValue(collectionId, out var collection))
        {
            ApiLog.Warn("Database Director", $"&3[RemoveItemRequest]&r Unknown collection: {collectionId}");
            return;
        }
        
        ApiLog.Debug("Database Director", $"&3[RemoveItemRequest]&r Removing {itemCount} item(s) in {collection.GetLogPath()}");

        for (var i = 0; i < itemCount; i++)
        {
            var itemName = reader.ReadString();
            
            ApiLog.Debug("Database Director", $"&3[RemoveItemRequest]&r Removing item {itemName}");

            if (collection.InternalTryGet(itemName, out var item))
            {
                collection.InternalRemoveItem(item);
                
                ApiLog.Debug("Database Director", $"&3[RemoveItemRequest]&r Item found and removed");
            }
            else
            {
                ApiLog.Debug("Database Director", $"&3[RemoveItemRequest]&r Item not found");
            }
        }
    }
}