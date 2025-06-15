using System.Net;

using NetworkLib;

namespace CentralAPI.ClientPlugin.Databases.Wrappers;

/// <summary>
/// Manages serialization of <see cref="IPEndPoint"/>.
/// </summary>
public class DatabaseIpEndPoint : DatabaseWrapper<IPEndPoint>
{
    /// <inheritdoc cref="DatabaseWrapper{T}.Read"/>
    public override void Read(NetworkReader reader, ref IPEndPoint value)
    {
        value = reader.ReadIpEndPoint();
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Write"/>
    public override void Write(NetworkWriter writer, ref IPEndPoint value)
    {
        writer.WriteIpEndPoint(value);
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Compare"/>
    public override bool Compare(ref IPEndPoint value, ref IPEndPoint other)
    {
        return Equals(value, other);
    }
}