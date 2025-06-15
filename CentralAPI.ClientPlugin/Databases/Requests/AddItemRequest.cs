using CentralAPI.ClientPlugin.Databases.Internal;

using LabExtended.Core;

using NetworkLib;
using NetworkLib.Pools;
using NetworkClient = CentralAPI.ClientPlugin.Network.NetworkClient;

namespace CentralAPI.ClientPlugin.Databases.Requests;

internal static class AddItemRequest
{
    private static ushort addItemCode;
    
    internal static void Init()
    {
        addItemCode = NetworkClient.Scp.GetRequestType("Database.AddItem");
        
        NetworkClient.Scp.HandleRequest("Database.AddItem", HandleRequest);
    }

    internal static void SendRequest(byte tableId, byte collectionId, string itemName, bool orOverride, Action<NetworkWriter> itemWriter)
    {
        ApiLog.Debug("Database Director", $"&3[AddItemRequest]&r Sending: TableId={tableId}; CollectionId={collectionId}; ItemName={itemName}; OrOverride={orOverride}");
        
        NetworkClient.Scp.Request(addItemCode, writer =>
        {
            writer.WriteByte(tableId);
            writer.WriteByte(collectionId);
            writer.WriteString(itemName);

            var itemValue = NetworkDataPool.GetWriter(itemWriter);
            
            writer.WriteWriter(itemValue);
            writer.WriteBool(orOverride);
            
            itemValue.Return();
        }, HandleResponse);
    }

    private static void HandleResponse(NetworkReader reader)
    {
        var result = reader.ReadByte();

        switch (result)
        {
            case 0:
                ApiLog.Debug("Database Director", "&3[AddItemRequest]&r &2OK&r");
                break;
            
            case 1:
                ApiLog.Debug("Database Director", "&3[AddItemRequest]&r &1Failed&r: &1Table not found&r");
                break;
            
            case 2:
                ApiLog.Debug("Database Director", "&3[AddItemRequest]&r &1Failed&r: &1Collection not found&r");
                break;
            
            case 3:
                ApiLog.Debug("Database Director", "&3[AddItemRequest]&r &1Failed&r: &1Item exists&r");
                break;
            
            case 4:
                ApiLog.Debug("Database Director", $"&3[AddItemRequest]&r &1Failed&r: &1{reader.ReadString()}&r");
                break;
            
            default:
                ApiLog.Debug("Database Director", $"&3[AddItemRequest]&r &1Unknown Result&r (&6{result}&r)");
                break;
        }
    }

    private static void HandleRequest(NetworkReader reader, NetworkWriter writer)
    {
        var tableId = reader.ReadByte();
        var collectionId = reader.ReadByte();
        
        var itemName = reader.ReadString();
        var itemValue = reader.ReadReader();

        var orOverride = reader.ReadBool();
        
        ApiLog.Debug("Database Director", $"&3[AddItemRequest]&r TableId={tableId}; CollectionId={collectionId}; ItemName={itemName}; ItemValue={itemValue.Count} byte(s); OrOverride={orOverride}");
        
        if (!DatabaseDirector.tables.TryGetValue(tableId, out var table))
        {
            ApiLog.Warn("Database Director", $"&3[AddItemRequest]&r Unknown table (&1{tableId}&r)");
            return;
        }

        if (!table.collections.TryGetValue(collectionId, out var collection))
        {
            ApiLog.Warn("Database Director", $"&3[AddItemRequest]&r Unknown collection (&1{collectionId}&r)");
            return;
        }

        if (collection.InternalTryGet(itemName, out var item))
        {
            if (!orOverride)
            {
                ApiLog.Debug("Database Director", "&3[AddItemRequest]&r Override disabled and item exists");
                return;
            }

            item.reader = itemValue;
            item.hasRead = false;
            
            collection.InternalUpdate(item);
            
            ApiLog.Debug("Database Director", $"&3[AddItemRequest]&r Updated item &2{item.GetLogPath()}&r");
        }
        else
        {
            item = Activator.CreateInstance(collection.ItemType) as DatabaseItemBase;
            
            item.reader = itemValue;
            item.hasRead = false;
            
            item.Name = itemName;
            
            collection.InternalAddItem(item);
            
            ApiLog.Debug("Database Director", $"&3[AddItemRequest]&r Added item &2{item.GetLogPath()}&r");
        }
    }
}