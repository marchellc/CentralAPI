using CommonLib;

using NetworkLib;
using NetworkLib.Pools;

namespace CentralAPI.ServerApp.Databases.Requests;

internal static class RemoveItemRequest
{
    private static ushort removeItemCode;
    
    internal static void CreateCodes(ScpInstance instance)
    {
        removeItemCode = instance.GetRequestType("Database.RemoveItem");
    }

    internal static void Handle(ScpInstance instance, NetworkReader reader, NetworkWriter writer)
    {
        try
        {
            var tableId = reader.ReadByte();
            var collectionId = reader.ReadByte();

            if (!DatabaseDirector.tables.TryGetValue(tableId, out var table))
                return;

            if (!table.collections.TryGetValue(collectionId, out var collection))
                return;

            var itemCount = reader.ReadByte();
            var itemList = new List<string>();

            for (var i = 0; i < itemCount; i++)
            {
                var itemName = reader.ReadString();
                
                if (!collection.items.TryRemove(itemName, out var item))
                    continue;
                
                itemList.Add(itemName);

                item.writer?.Return();
                item.writer = null;

                try
                {
                    if (File.Exists(item.path))
                    {
                        File.Delete(item.path);
                    }
                }
                catch
                {
                    // ignored
                }
            }

            DatabaseDirector.SendToOthers(instance, removeItemCode, x =>
            {
                x.WriteByte(tableId);
                x.WriteByte(collectionId);
                
                x.WriteByte((byte)itemList.Count);
                
                foreach (var item in itemList)
                    writer.WriteString(item);
            });
        }
        catch (Exception ex)
        {
            CommonLog.Error("Database Director", $"An error occured while handling 'RemoveItemRequest':\n{ex}");
        }
    }
}