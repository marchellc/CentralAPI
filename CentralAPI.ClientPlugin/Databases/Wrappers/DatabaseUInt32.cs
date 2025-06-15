using NetworkLib;

namespace CentralAPI.ClientPlugin.Databases.Wrappers;

/// <summary>
/// Manages serialization of unsigned 32-bit integer values.
/// </summary>
public class DatabaseUInt32 : DatabaseWrapper<uint>
{
    /// <inheritdoc cref="DatabaseWrapper{T}.Read"/>
    public override void Read(NetworkReader reader, ref uint value)
    {
        value = reader.ReadUInt();
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Write"/>
    public override void Write(NetworkWriter writer, ref uint value)
    {
        writer.WriteUInt(value);
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Compare"/>
    public override bool Compare(ref uint value, ref uint other)
    {
        return value == other;
    }
}