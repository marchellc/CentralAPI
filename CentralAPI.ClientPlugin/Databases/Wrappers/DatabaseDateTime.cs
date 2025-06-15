using NetworkLib;

namespace CentralAPI.ClientPlugin.Databases.Wrappers;

/// <summary>
/// Manages serialization of <see cref="DateTime"/>.
/// </summary>
public class DatabaseDateTime : DatabaseWrapper<DateTime>
{
    /// <inheritdoc cref="DatabaseWrapper{T}.Read"/>
    public override void Read(NetworkReader reader, ref DateTime value)
    {
        value = new DateTime(reader.ReadLong());
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Write"/>
    public override void Write(NetworkWriter writer, ref DateTime value)
    {
        writer.WriteLong(value.Ticks);
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Compare"/>
    public override bool Compare(ref DateTime value, ref DateTime other)
    {
        return value == other;
    }
}