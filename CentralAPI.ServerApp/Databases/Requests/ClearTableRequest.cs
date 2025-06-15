using CommonLib;

using NetworkLib;
using NetworkLib.Pools;

namespace CentralAPI.ServerApp.Databases.Requests;

internal static class ClearTableRequest
{
    private static ushort clearTableCode;

    internal static void CreateCodes(ScpInstance instance)
    {
        clearTableCode = instance.GetRequestType("Database.ClearTable");
    }

    // 0 - OK
    // 1 - Table not found
    // 2 - Exception
    internal static void Handle(ScpInstance instance, NetworkReader reader, NetworkWriter writer)
    {
        try
        {
            CommonLog.Debug("Database Director", $"[ClearTableRequest] Server={instance.Port}; Reader={reader?.Count ?? -1}");
            
            var tableId = reader.ReadByte();
            var deleteTable = reader.ReadBool();
            
            CommonLog.Debug("Database Director", $"[ClearTableRequest] TableId={tableId}; DeleteTable={deleteTable}");

            if (!DatabaseDirector.tables.TryGetValue(tableId, out var table))
            {
                CommonLog.Debug("Database Director", $"[ClearTableRequest] Table not found");
                
                writer.WriteByte(1);
                return;
            }

            foreach (var collection in table.collections)
            {
                table.collections.TryRemove(collection.Key, out _);
                
                foreach (var item in collection.Value.items)
                    item.Value.writer?.Return();
                
                collection.Value.items.Clear();
                
                try
                {
                    Directory.Delete(collection.Value.path, true);
                }
                catch
                {
                    // ignored
                }
            }
            
            table.collections.Clear();

            if (deleteTable)
            {
                DatabaseDirector.tables.TryRemove(tableId, out _);

                try
                {
                    Directory.Delete(table.path, true);
                }
                catch
                {
                    // ignored
                }
            }
            
            CommonLog.Debug("Database Director", $"[ClearTableRequest] Cleared / removed table");
            
            writer.WriteByte(0);
            
            DatabaseDirector.SendToOthers(instance, clearTableCode, x =>
            {
                x.WriteByte(tableId);
                x.WriteBool(deleteTable);
            });
        }
        catch (Exception ex)
        {
            CommonLog.Error("Database Director", $"Could not handle 'ClearCollectionRequest':\n{ex}");
            
            writer.WriteByte(2);
            writer.WriteString(ex.Message);
        }
    }
}