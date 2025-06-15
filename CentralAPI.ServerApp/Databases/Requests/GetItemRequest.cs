using CommonLib;

using NetworkLib;
using NetworkLib.Pools;

namespace CentralAPI.ServerApp.Databases.Requests;

internal static class GetItemRequest
{
    internal static ushort getItemCode;

    internal static void CreateCodes(ScpInstance instance)
    {
        getItemCode = instance.GetRequestType("Database.GetItem");
    }

    // 0 - Item found
    // 1 - Item added
    // 2 - Table not found
    // 3 - Collection not found
    // 4 - Item not found
    // 5 - Exception
    internal static void Handle(ScpInstance instance, NetworkReader reader, NetworkWriter writer)
    {
        try
        {
            var tableId = reader.ReadByte();
            var collectionId = reader.ReadByte();
            
            var itemName = reader.ReadString();
            var itemValue = default(NetworkWriter);
            
            var orAdd = reader.ReadBool();

            if (orAdd)
                itemValue = reader.ReadWriter();

            if (!DatabaseDirector.tables.TryGetValue(tableId, out var table))
            {
                itemValue?.Return();

                writer.WriteByte(2);
                return;
            }

            if (!table.collections.TryGetValue(collectionId, out var collection))
            {
                itemValue?.Return();
                
                writer.WriteByte(3);
                return;
            }

            if (!collection.items.TryGetValue(itemName, out var item))
            {
                if (orAdd)
                {
                    item = new();
                    
                    item.name = itemName;
                    item.path = Path.Combine(collection.path, itemName + ".db");
                    item.writer = itemValue;
                    
                    collection.items.TryAdd(itemName, item);
                    
                    File.WriteAllBytes(item.path, itemValue.Buffer.ToArray());
                    
                    writer.WriteByte(1);
                    
                    DatabaseDirector.SendToOthers(instance, getItemCode, x =>
                    {
                        x.WriteByte(tableId);
                        x.WriteByte(collectionId);
                        
                        x.WriteString(itemName);
                        x.WriteWriter(itemValue);
                        
                        x.WriteBool(true);
                    });
                    
                    return;
                }
                
                writer.WriteByte(4);
                return;
            }

            writer.WriteByte(0);
            writer.WriteWriter(item.writer);
        }
        catch (Exception ex)
        {
            CommonLog.Error("Database Director", $"Could not handle 'GetItemRequest':\n{ex}");
            
            writer.WriteByte(5);
            writer.WriteString(ex.Message);
        }
    }
}