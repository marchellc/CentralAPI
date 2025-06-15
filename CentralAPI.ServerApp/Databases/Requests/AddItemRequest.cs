using CentralAPI.ServerApp.Extensions;
using CommonLib;

using NetworkLib;
using NetworkLib.Pools;

namespace CentralAPI.ServerApp.Databases.Requests;

internal static class AddItemRequest
{
    private static ushort addItemCode;
    
    internal static void CreateCodes(ScpInstance instance)
    {
        addItemCode = instance.GetRequestType("Database.AddItem");
    }

    // 0 - OK
    // 1 - Table not found
    // 2 - Collection not found
    // 3 - Item exists
    // 4 - Exception
    internal static void Handle(ScpInstance instance, NetworkReader reader, NetworkWriter writer)
    {
        try
        {
            CommonLog.Debug("Database Director", $"[AddItemRequest] Server={instance.Port}; Reader={reader?.Count ?? -1}");
            
            var tableId = reader.ReadByte();
            var collectionId = reader.ReadByte();
            
            var itemName = reader.ReadString();
            var itemValue = reader.ReadWriter();

            var orOverride = reader.ReadBool();
            
            CommonLog.Debug("Database Director", $"[AddItemRequest] TableId={tableId}; CollectionId={collectionId}; ItemName={itemName}; ItemValue={itemValue?.Count ?? -1}; OrOverride={orOverride}");
            
            if (!DatabaseDirector.tables.TryGetValue(tableId, out var table))
            {
                CommonLog.Debug("Database Director", $"[AddItemRequest] Table not found");
                
                itemValue.Return();
                
                writer.WriteByte(1);
                return;
            }

            if (!table.collections.TryGetValue(collectionId, out var collection))
            {
                CommonLog.Debug("Database Director", $"[AddItemRequest] Collection not found");
                
                itemValue.Return();
                
                writer.WriteByte(2);
                return;
            }

            if (collection.items.TryRemove(itemName, out var curItem) && (!orOverride || curItem.writer.IsEqual(itemValue)))
            {
                CommonLog.Debug("Database Director", $"[AddItemRequest] Item exists");
                
                itemValue.Return();
                
                collection.items.TryAdd(itemName, curItem);
                
                writer.WriteByte(3);
                return;
            }

            var item = new DatabaseItem();

            item.name = itemName;
            item.writer = itemValue;
            
            item.path = Path.Combine(collection.path, item.name + ".db");
            
            collection.items.TryAdd(itemName, item);
            
            File.WriteAllBytes(item.path, itemValue.Buffer.ToArray());
            
            CommonLog.Debug("Database Director", $"[AddItemRequest] Added new item (path: {item.path})");
            
            writer.WriteByte(0);
            
            DatabaseDirector.SendToOthers(instance, addItemCode, x =>
            {
                x.WriteByte(tableId);
                x.WriteByte(collectionId);
                x.WriteString(itemName);
                x.WriteWriter(itemValue);
                x.WriteBool(orOverride);
            });
        }
        catch (Exception ex)
        {
            CommonLog.Error("Database Director", $"Could not handle 'AddItemRequest':\n{ex}");
            
            writer.WriteByte(4);
            writer.WriteString(ex.Message);
        }
    }
}