using NetworkLib;
using NetworkLib.Pools;
using NetworkLib.Interfaces; 

namespace CentralAPI.SharedLib.Requests;

public struct RequestMessage : INetworkMessage
{
    public ushort RequestId;
    public ushort RequestType;
    
    public NetworkReader Reader;
    public NetworkWriter Writer;

    public RequestMessage(ushort requestId, ushort requestType, NetworkWriter writer)
    {
        RequestId = requestId;
        RequestType = requestType;
        
        Writer = writer;
    }

    public void Read(NetworkReader reader)
    {
        RequestId = reader.ReadUShort();
        RequestType = reader.ReadUShort();

        if (reader.ReadBool())
            Reader = reader.ReadReader();
    }

    public void Write(NetworkWriter writer)
    {
        writer.WriteUShort(RequestId);
        writer.WriteUShort(RequestType);

        if (Writer != null)
        {
            writer.WriteBool(true);
            writer.WriteWriter(Writer);
            
            NetworkDataPool.Return(Writer);

            Writer = null;
        }
        else
        {
            writer.WriteBool(false);
        }
    }
}