using NetworkLib;

namespace CentralAPI.ClientPlugin.Databases.Wrappers;

/// <summary>
/// Manages serialization of unsigned 64-bit integer values.
/// </summary>
public class DatabaseUInt64 : DatabaseWrapper<ulong>
{
    /// <inheritdoc cref="DatabaseWrapper{T}.Read"/>
    public override void Read(NetworkReader reader, ref ulong value)
    {
        value = reader.ReadULong();
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Write"/>
    public override void Write(NetworkWriter writer, ref ulong value)
    {
        writer.WriteULong(value);
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Compare"/>
    public override bool Compare(ref ulong value, ref ulong other)
    {
        return value == other;
    }
    
    /// <inheritdoc cref="DatabaseWrapper{T}.Convert"/>
    public override void Convert(ulong value, out string result)
    {
        result = value.ToString();
    }
}