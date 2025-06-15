using CommonLib;

using NetworkLib;

namespace CentralAPI.ServerApp.Databases.Requests;

internal static class EnsureRequest
{
    // 0 - OK
    // 1 - Exception
    internal static void Handle(ScpInstance instance, NetworkReader reader, NetworkWriter writer)
    {
        try
        {
            CommonLog.Debug("Database Director", $"[EnsureRequest] Server={instance.Port}; Reader={reader?.Count ?? -1}");
            
            var tableId = reader.ReadByte();
            
            var collectionId = reader.ReadByte();
            var collectionType = reader.ReadString();

            CommonLog.Debug("Database Director", $"[EnsureRequest] TableId={tableId}; CollectionId={collectionId}; CollectionType={collectionType}");
            
            if (!DatabaseDirector.tables.TryGetValue(tableId, out var table))
            {
                table = new();

                table.id = tableId;
                table.path = Path.Combine(DatabaseDirector.path, tableId.ToString());
                
                if (!Directory.Exists(table.path))
                    Directory.CreateDirectory(table.path);
                
                DatabaseDirector.tables.TryAdd(tableId, table);
                
                CommonLog.Debug("Database Director", $"[EnsureRequest] Added table");
            }
            else
            {
                CommonLog.Debug("Database Director", $"[EnsureRequest] Table exists");
            }

            if (!table.collections.TryGetValue(collectionId, out var collection))
            {
                collection = new();

                collection.id = collectionId;
                collection.table = table;
                collection.path = Path.Combine(table.path, collectionId.ToString());
                collection.type = collectionType;

                if (!Directory.Exists(collection.path))
                    Directory.CreateDirectory(collection.path);
                
                File.WriteAllText(Path.Combine(collection.path, "type.txt"), collectionType);
                
                table.collections.TryAdd(collectionId, collection);
                
                CommonLog.Debug("Database Director", $"[EnsureRequest] Added collection");
            }
            else
            {
                CommonLog.Debug("Database Director", $"[EnsureRequest] Collection exists");
            }
            
            writer.WriteByte(0);
        }
        catch (Exception ex)
        {
            CommonLog.Error("Database Director", $"Could not handle 'EnsureRequest':\n{ex}");
            
            writer.WriteByte(1);
            writer.WriteString(ex.Message);
        }
    }
}