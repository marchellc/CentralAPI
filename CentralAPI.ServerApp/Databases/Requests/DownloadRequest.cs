using CommonLib; 

using NetworkLib;

namespace CentralAPI.ServerApp.Databases.Requests;

internal static class DownloadRequest
{
    internal static void Handle(ScpInstance instance, NetworkReader reader, NetworkWriter writer)
    {
        try
        {
            CommonLog.Debug("Database Director", $"[DownloadRequest] Server={instance.Port}; Reader={reader?.Count ?? -1}");
            
            writer.WriteByte((byte)DatabaseDirector.tables.Count);
            
            CommonLog.Debug("Database Director", $"[DownloadRequest] Tables: {DatabaseDirector.tables.Count}");
            
            foreach (var table in DatabaseDirector.tables)
            {
                writer.WriteByte(table.Key);
                writer.WriteByte((byte)table.Value.collections.Count);
                
                CommonLog.Debug("Database Director", $"[DownloadRequest] Writing table {table.Key} ({table.Value.collections.Count})");

                foreach (var collection in table.Value.collections)
                {
                    writer.WriteByte(collection.Key);
                    writer.WriteInt(collection.Value.items.Count);
                    writer.WriteString(collection.Value.type);

                    CommonLog.Debug("Database Director", $"[DownloadRequest] Writing collection {collection.Key} ({collection.Value.items.Count})");
                    
                    foreach (var item in collection.Value.items)
                    {
                        writer.WriteString(item.Key);
                        writer.WriteWriter(item.Value.writer);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            CommonLog.Error("Database Director", $"Could not handle 'DownloadRequest':\n{ex}");
        }
    }
}