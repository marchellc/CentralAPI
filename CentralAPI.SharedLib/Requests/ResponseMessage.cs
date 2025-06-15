using NetworkLib;
using NetworkLib.Pools;
using NetworkLib.Interfaces; 

namespace CentralAPI.SharedLib.Requests;

public struct ResponseMessage : INetworkMessage
{
    public ushort RequestId;

    public NetworkWriter Writer;
    public NetworkReader Reader;

    public ResponseMessage(ushort requestId, NetworkWriter writer)
    {
        RequestId = requestId;
        Writer = writer;
    }

    public void Read(NetworkReader reader)
    {
        RequestId = reader.ReadUShort();

        if (reader.ReadBool())
            Reader = reader.ReadReader();
    }

    public void Write(NetworkWriter writer)
    {
        writer.WriteUShort(RequestId);

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