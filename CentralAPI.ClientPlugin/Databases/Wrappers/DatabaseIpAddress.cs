using System.Net;

using NetworkLib;

namespace CentralAPI.ClientPlugin.Databases.Wrappers;

/// <summary>
/// Manages serialization of <see cref="IPAddress"/>.
/// </summary>
public class DatabaseIpAddress : DatabaseWrapper<IPAddress>
{
    /// <inheritdoc cref="DatabaseWrapper{T}.Read"/>
    public override void Read(NetworkReader reader, ref IPAddress value)
    {
        value = reader.ReadIpAddress();
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Write"/>
    public override void Write(NetworkWriter writer, ref IPAddress value)
    {
        writer.WriteIpAddress(value);
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Compare"/>
    public override bool Compare(ref IPAddress value, ref IPAddress other)
    {
        return Equals(value, other);
    }
    
    /// <inheritdoc cref="DatabaseWrapper{T}.Convert"/>
    public override void Convert(IPAddress value, out string result)
    {
        if (value is null)
        {
            result = "(null)";
            return;
        }

        result = value.ToString();
    }
}