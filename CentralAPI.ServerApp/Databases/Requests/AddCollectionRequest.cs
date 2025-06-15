using CommonLib;

using NetworkLib;

namespace CentralAPI.ServerApp.Databases.Requests;

internal static class AddCollectionRequest
{
    private static ushort addCollectionCode;

    internal static void CreateCodes(ScpInstance instance)
    {
        addCollectionCode = instance.GetRequestType("Database.AddCollection");
    }

    // 0 - OK
    // 1 - Exception
    internal static void Handle(ScpInstance instance, NetworkReader reader, NetworkWriter writer)
    {
        try
        {
            CommonLog.Debug("Database Director", $"[AddCollectionRequest] Server={instance.Port}; Reader={reader?.Count ?? -1}");
            
            var tableId = reader.ReadByte();
            
            var collectionId = reader.ReadByte();
            var collectionType = reader.ReadString();
            
            CommonLog.Debug("Database Director", $"[AddCollectionRequest] TableId={tableId}; CollectionId={collectionId}; Type={collectionType}");

            if (!DatabaseDirector.tables.TryGetValue(tableId, out var table))
            {
                table = new();
            
                table.id = tableId;
                table.path = Path.Combine(DatabaseDirector.path, table.ToString());

                if (!Directory.Exists(table.path))
                    Directory.CreateDirectory(table.path);
            
                DatabaseDirector.tables.TryAdd(table.id, table);
                
                CommonLog.Debug("Database Director", $"[AddCollectionRequest] Added new table {tableId}");
            }
            else
            {
                CommonLog.Debug("Database Director", $"[AddCollectionRequest] Table {tableId} already exists");
            }

            if (!table.collections.TryGetValue(collectionId, out var collection))
            {
                collection = new();
                collection.table = table;
                
                collection.id = collectionId;
                collection.type = collectionType;

                collection.path = Path.Combine(table.path, collectionId.ToString());

                if (!Directory.Exists(collection.path))
                    Directory.CreateDirectory(collection.path);
                
                File.WriteAllText(Path.Combine(collection.path, "type.txt"), collectionType);
                
                table.collections.TryAdd(collectionId, collection);
                
                CommonLog.Debug("Database Director", $"[AddCollectionRequest] Added new collection {collectionId}");
            }
            else
            {
                CommonLog.Debug("Database Director", $"[AddCollectionRequest] Collection {collectionId} already exists");
            }
            
            writer.WriteByte(0);
            
            DatabaseDirector.SendToOthers(instance, addCollectionCode, x =>
            {
                x.WriteByte(tableId);
                x.WriteByte(collectionId);
                
                x.WriteString(collectionType);
            });
        }
        catch (Exception ex)
        {
            writer.Clear();
            
            writer.WriteByte(1);
            writer.WriteString(ex.Message);
        }
    }
}