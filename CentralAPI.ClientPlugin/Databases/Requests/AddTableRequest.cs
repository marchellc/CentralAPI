using LabExtended.Core;

using NetworkLib;
using NetworkClient = CentralAPI.ClientPlugin.Network.NetworkClient;

namespace CentralAPI.ClientPlugin.Databases.Requests;

internal static class AddTableRequest
{
    private static ushort addTableCode;
    
    internal static void Init()
    {
        addTableCode = NetworkClient.Scp.GetRequestType("Database.AddTable");
        
        NetworkClient.Scp.HandleRequest("Database.AddTable", HandleRequest);
    }

    internal static void SendRequest(byte tableId)
    {
        ApiLog.Debug("Database Director", $"&3[AddTableRequest]&r SENDING: TableId={tableId}");
        
        NetworkClient.Scp.Request(addTableCode, writer => writer.WriteByte(tableId), HandleResponse);
    }

    private static void HandleResponse(NetworkReader reader)
    {
        if (reader.ReadByte() != 0)
            ApiLog.Debug("Database Director", $"&3[AddTableRequest]&r &1{reader.ReadString()}&r");
        else
            ApiLog.Debug("Database Director", $"&3[AddTableRequest]&r &2OK&r");
    }

    private static void HandleRequest(NetworkReader reader, NetworkWriter writer)
    {
        var tableId = reader.ReadByte();

        ApiLog.Debug("Database Director", $"&3[AddTableRequest]&r RECEIVED: TableId={tableId}");
        
        if (DatabaseDirector.tables.TryGetValue(tableId, out var table))
        {
            ApiLog.Debug("Database Director", $"&3[AddTableRequest]&r Table {tableId} exists");
            return;
        }

        table = new();
        table.Id = tableId;
        
        DatabaseDirector.tables.Add(tableId, table);
        DatabaseDirector.OnAdded(table);
        
        ApiLog.Debug("Database Director", $"&3[AddTableRequest]&r Added table {tableId}");
    }
}