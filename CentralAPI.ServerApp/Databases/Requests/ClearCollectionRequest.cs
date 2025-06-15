using CommonLib;

using NetworkLib;
using NetworkLib.Pools;

namespace CentralAPI.ServerApp.Databases.Requests;

internal static class ClearCollectionRequest
{
    private static ushort clearCollectionCode;

    internal static void CreateCodes(ScpInstance instance)
    {
        clearCollectionCode = instance.GetRequestType("Database.ClearCollection");
    }

    // 0 - OK
    // 1 - Unknown table
    // 2 - Unknown collection
    // 3 - Exception
    internal static void Handle(ScpInstance instance, NetworkReader reader, NetworkWriter writer)
    {
        try
        {
            CommonLog.Debug("Database Director", $"[ClearCollectionRequest] Server={instance.Port}; Reader={reader?.Count ?? -1}");
            
            var tableId = reader.ReadByte();
            var collectionId = reader.ReadByte();
            var deleteCollection = reader.ReadBool();

            CommonLog.Debug("Database Director", $"[ClearCollectionRequest] TableId={tableId}; CollectionId={collectionId}; DeleteCollection={deleteCollection}");
            
            if (!DatabaseDirector.tables.TryGetValue(tableId, out var table))
            {
                CommonLog.Debug("Database Director", $"[ClearCollectionRequest] Table not found");
                
                writer.WriteByte(1);
                return;
            }

            if (!table.collections.TryGetValue(collectionId, out var collection))
            {
                CommonLog.Debug("Database Director", $"[ClearCollectionRequest] Collection not found");
                
                writer.WriteByte(2);
                return;
            }

            foreach (var item in collection.items)
                item.Value.writer?.Return();
            
            collection.items.Clear();

            if (deleteCollection)
            {
                table.collections.TryRemove(collectionId, out _);
                
                try
                {
                    Directory.Delete(collection.path, true);
                }
                catch
                {
                    // ignored
                }
            }
            
            CommonLog.Debug("Database Director", $"[ClearCollectionRequest] Removed / cleared collection");
            
            writer.WriteByte(0);
            
            DatabaseDirector.SendToOthers(instance, clearCollectionCode, x =>
            {
                x.WriteByte(tableId);
                x.WriteByte(collectionId);
                x.WriteBool(deleteCollection);
            });
        }
        catch (Exception ex)
        {
            CommonLog.Error("Database Director", $"Could not handle 'ClearCollectionRequest':\n{ex}");
            
            writer.WriteByte(3);
            writer.WriteString(ex.Message);
        }
    }
}