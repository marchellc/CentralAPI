using CommonLib;

using NetworkLib;

namespace CentralAPI.ServerApp.Databases.Requests;

internal static class AddTableRequest
{
    private static ushort addTableCode;

    internal static void CreateCodes(ScpInstance instance)
    {
        addTableCode = instance.GetRequestType("Database.AddTable");
    }

    // 0 - OK
    // 1 - Exception
    internal static void Handle(ScpInstance instance, NetworkReader reader, NetworkWriter writer)
    {
        try
        {
            CommonLog.Debug("Database Director", $"[AddTableRequest] Server={instance.Port}; Reader={reader?.Count ?? -1}");
            
            var tableId = reader.ReadByte();
            
            CommonLog.Debug("Database Director", $"[AddTableRequest] TableId={tableId}");

            if (!DatabaseDirector.tables.TryGetValue(tableId, out var table))
            {
                table = new();

                table.id = tableId;
                table.path = Path.Combine(DatabaseDirector.path, tableId.ToString());

                if (!Directory.Exists(table.path))
                    Directory.CreateDirectory(table.path);

                DatabaseDirector.tables.TryAdd(table.id, table);
                
                CommonLog.Debug("Database Director", $"[AddTableRequest] Added table {tableId}");
            }
            else
            {
                CommonLog.Debug("Database Director", $"[AddTableRequest] Table {tableId} already exists");
            }

            writer.WriteByte(0);
            
            DatabaseDirector.SendToOthers(instance, addTableCode, x =>
            {
                x.WriteByte(tableId);
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