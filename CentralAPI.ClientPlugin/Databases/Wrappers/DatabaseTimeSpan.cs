using NetworkLib;

namespace CentralAPI.ClientPlugin.Databases.Wrappers;

/// <summary>
/// Manages serialization of <see cref="TimeSpan"/>.
/// </summary>
public class DatabaseTimeSpan : DatabaseWrapper<TimeSpan>
{
    /// <inheritdoc cref="DatabaseWrapper{T}.Read"/>
    public override void Read(NetworkReader reader, ref TimeSpan value)
    {
        value = new TimeSpan(reader.ReadLong());
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Write"/>
    public override void Write(NetworkWriter writer, ref TimeSpan value)
    {
        writer.WriteLong(value.Ticks);
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Compare"/>
    public override bool Compare(ref TimeSpan value, ref TimeSpan other)
    {
        return value == other;
    }
}