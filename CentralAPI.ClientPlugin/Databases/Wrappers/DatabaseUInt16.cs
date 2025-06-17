using NetworkLib;

namespace CentralAPI.ClientPlugin.Databases.Wrappers;

/// <summary>
/// Manages serialization of unsigned 16-bit integer values.
/// </summary>
public class DatabaseUInt16 : DatabaseWrapper<ushort>
{
    /// <inheritdoc cref="DatabaseWrapper{T}.Read"/>
    public override void Read(NetworkReader reader, ref ushort value)
    {
        value = reader.ReadUShort();
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Write"/>
    public override void Write(NetworkWriter writer, ref ushort value)
    {
        writer.WriteUShort(value);
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Compare"/>
    public override bool Compare(ref ushort value, ref ushort other)
    {
        return value == other;
    }
    
    /// <inheritdoc cref="DatabaseWrapper{T}.Convert"/>
    public override void Convert(ushort value, out string result)
    {
        result = value.ToString();
    }
}