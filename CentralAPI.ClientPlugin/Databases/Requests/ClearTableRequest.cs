using LabExtended.Core;

using NetworkLib;
using NetworkClient = CentralAPI.ClientPlugin.Network.NetworkClient;

namespace CentralAPI.ClientPlugin.Databases.Requests;

internal static class ClearTableRequest
{
    private static ushort clearTableCode;
    
    internal static void Init()
    {
        clearTableCode = NetworkClient.Scp.GetRequestType("Database.ClearTable");
        
        NetworkClient.scp.HandleRequest("Database.ClearTable", HandleRequest);
    }

    internal static void SendRequest(byte tableId, bool drop)
    {
        ApiLog.Debug("Database Director", $"&3[ClearTableRequest]&r SENDING: TableId={tableId}; Drop={drop}");
        
        NetworkClient.Scp.Request(clearTableCode, writer =>
        {
            writer.WriteByte(tableId);
            writer.WriteBool(drop);
        }, HandleResponse);
    }

    private static void HandleResponse(NetworkReader reader)
    {
        var result = reader.ReadByte();

        switch (result)
        {
            case 0:
                ApiLog.Debug("Database Director", $"&3[ClearTableRequest]&r &2OK&r");
                break;
            
            case 1:
                ApiLog.Debug("Database Director", $"&3[ClearTableRequest]&r &1Unknown Table&r");
                break;
            
            case 2:
                ApiLog.Debug("Database Director", $"&3[ClearTableRequest]&r &1{reader.ReadString()}&r");
                break;
        }
    }
    
    private static void HandleRequest(NetworkReader reader, NetworkWriter writer)
    {
        var tableId = reader.ReadByte();
        var drop = reader.ReadBool();
        
        ApiLog.Debug("Database Director", $"&3[ClearTableRequest]&r RECEIVED: TableId={tableId}; Drop={drop}");

        if (!DatabaseDirector.TryGetTable(tableId, out var table))
        {
            ApiLog.Warn("Database Director", $"&3[ClearTableRequest]&r Unknown table: {tableId}");
            return;
        }

        if (drop)
        {
            table.InternalDestroy(true);
            
            DatabaseDirector.tables.Remove(tableId);
            
            ApiLog.Debug("Database Director", $"&3[ClearTableRequest]&r Dropped {table.Id}");
        }
        else
        {
            table.InternalClear();
            
            ApiLog.Debug("Database Director", $"&3[ClearTableRequest]&r Cleared {table.Id}");
        }
    }
}