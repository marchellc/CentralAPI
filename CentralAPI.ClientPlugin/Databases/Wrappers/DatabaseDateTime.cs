using CentralAPI.SharedLib;
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
        value = reader.ReadDate();
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Write"/>
    public override void Write(NetworkWriter writer, ref DateTime value)
    {
        writer.WriteDate(value);
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Compare"/>
    public override bool Compare(ref DateTime value, ref DateTime other)
    {
        return value == other;
    }
    
    /// <inheritdoc cref="DatabaseWrapper{T}.Convert"/>
    public override void Convert(DateTime value, out string result)
    {
        result = value.ToString();
    }
}