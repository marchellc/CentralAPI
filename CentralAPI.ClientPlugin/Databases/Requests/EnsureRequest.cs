using CentralAPI.ClientPlugin.Network;

using LabExtended.Core;

namespace CentralAPI.ClientPlugin.Databases.Requests;

internal static class EnsureRequest
{
    private static ushort code;
    
    internal static void SendRequest()
    {
        foreach (var pair in DatabaseDirector.RequiredCollections)
        {
            foreach (var collection in pair.Value)
            {
                NetworkClient.Scp.Request(code, writer =>
                {
                    writer.WriteByte(pair.Key);
                    writer.WriteByte(collection.Key);
                    writer.WriteString(collection.Value.AssemblyQualifiedName);
                }, reader =>
                {
                    var result = reader.ReadByte();

                    if (result != 0)
                    {
                        if (result == 1)
                        {
                            ApiLog.Warn("Database Director", $"&3[EnsureRequest]&r &6{pair.Key}&r / &6{collection}&r: &1{reader.ReadString()}&r");
                        }
                        else
                        {
                            ApiLog.Warn("Database Director", $"&3[EnsureRequest]&r &6{pair.Key}&r / &6{collection}&r: &1FAILED&r");
                        }
                    }
                    else
                    {
                        ApiLog.Debug("Database Director", $"&3[EnsureRequest]&r &6{pair.Key}&r / &6{collection}&r: &2OK&r");
                    }
                });
            }
        }
    }
    
    internal static void Init()
    {
        code = NetworkClient.scp.GetRequestType("Database.EnsureExistence");
    }
}