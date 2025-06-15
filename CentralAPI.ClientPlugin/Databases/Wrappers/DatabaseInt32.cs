using NetworkLib;

namespace CentralAPI.ClientPlugin.Databases.Wrappers;

/// <summary>
/// Manages serialization of signed 32-bit integer values.
/// </summary>
public class DatabaseInt32 : DatabaseWrapper<int>
{
    /// <inheritdoc cref="DatabaseWrapper{T}.Read"/>
    public override void Read(NetworkReader reader, ref int value)
    {
        value = reader.ReadInt();
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Write"/>
    public override void Write(NetworkWriter writer, ref int value)
    {
        writer.WriteInt(value);
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Compare"/>
    public override bool Compare(ref int value, ref int other)
    {
        return value == other;
    }
}