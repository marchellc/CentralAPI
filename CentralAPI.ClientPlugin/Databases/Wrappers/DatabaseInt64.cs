using NetworkLib;

namespace CentralAPI.ClientPlugin.Databases.Wrappers;

/// <summary>
/// Manages serialization of signed 64-bit integer values.
/// </summary>
public class DatabaseInt64 : DatabaseWrapper<long>
{
    /// <inheritdoc cref="DatabaseWrapper{T}.Read"/>
    public override void Read(NetworkReader reader, ref long value)
    {
        value = reader.ReadLong();
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Write"/>
    public override void Write(NetworkWriter writer, ref long value)
    {
        writer.WriteLong(value);
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Compare"/>
    public override bool Compare(ref long value, ref long other)
    {
        return value == other;
    }
    
    /// <inheritdoc cref="DatabaseWrapper{T}.Convert"/>
    public override void Convert(long value, out string result)
    {
        result = value.ToString();
    }
}