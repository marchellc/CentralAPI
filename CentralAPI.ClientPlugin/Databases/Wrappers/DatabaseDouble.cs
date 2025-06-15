using NetworkLib;

namespace CentralAPI.ClientPlugin.Databases.Wrappers;

/// <summary>
/// Manages serialization of <see cref="double"/>.
/// </summary>
public class DatabaseDouble : DatabaseWrapper<double>
{
    /// <inheritdoc cref="DatabaseWrapper{T}.Read"/>
    public override void Read(NetworkReader reader, ref double value)
    {
        value = reader.ReadDouble();
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Write"/>
    public override void Write(NetworkWriter writer, ref double value)
    {
        writer.WriteDouble(value);
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Compare"/>
    public override bool Compare(ref double value, ref double other)
    {
        return value == other;
    }
}