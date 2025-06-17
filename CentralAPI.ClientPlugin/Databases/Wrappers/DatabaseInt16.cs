using NetworkLib;

namespace CentralAPI.ClientPlugin.Databases.Wrappers;

/// <summary>
/// Manages serialization of signed 16-bit integer values.
/// </summary>
public class DatabaseInt16 : DatabaseWrapper<short>
{
    /// <inheritdoc cref="DatabaseWrapper{T}.Read"/>
    public override void Read(NetworkReader reader, ref short value)
    {
        value = reader.ReadShort();
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Write"/>
    public override void Write(NetworkWriter writer, ref short value)
    {
        writer.WriteShort(value);
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Compare"/>
    public override bool Compare(ref short value, ref short other)
    {
        return value == other;
    }
    
    /// <inheritdoc cref="DatabaseWrapper{T}.Convert"/>
    public override void Convert(short value, out string result)
    {
        result = value.ToString();
    }
}