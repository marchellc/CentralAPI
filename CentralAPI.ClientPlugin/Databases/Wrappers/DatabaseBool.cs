using NetworkLib;

namespace CentralAPI.ClientPlugin.Databases.Wrappers;

/// <summary>
/// Manages serialization of boolean values.
/// </summary>
public class DatabaseBool : DatabaseWrapper<bool>
{
    /// <inheritdoc cref="DatabaseWrapper{T}.Read"/>
    public override void Read(NetworkReader reader, ref bool value)
    {
        value = reader.ReadBool();
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Write"/>
    public override void Write(NetworkWriter writer, ref bool value)
    {
        writer.WriteBool(value);
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Compare"/>
    public override bool Compare(ref bool value, ref bool other)
    {
        return value == other;
    }
}