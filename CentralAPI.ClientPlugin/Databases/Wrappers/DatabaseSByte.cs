using NetworkLib;

namespace CentralAPI.ClientPlugin.Databases.Wrappers;

/// <summary>
/// Manages serialization of signed byte values.
/// </summary>
public class DatabaseSByte : DatabaseWrapper<sbyte>
{
    /// <inheritdoc cref="DatabaseWrapper{T}.Read"/>
    public override void Read(NetworkReader reader, ref sbyte value)
    {
        value = (sbyte)reader.ReadShort();
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Write"/>
    public override void Write(NetworkWriter writer, ref sbyte value)
    {
        writer.WriteShort(value);
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Compare"/>
    public override bool Compare(ref sbyte value, ref sbyte other)
    {
        return value == other;
    }
}