using NetworkLib;

namespace CentralAPI.ClientPlugin.Databases.Wrappers;

/// <summary>
/// Manages serialization of strings.
/// </summary>
public class DatabaseString : DatabaseWrapper<string>
{
    /// <inheritdoc cref="DatabaseWrapper{T}.Read"/>
    public override void Read(NetworkReader reader, ref string value)
    {
        value = reader.ReadString();
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Write"/>
    public override void Write(NetworkWriter writer, ref string value)
    {
        writer.WriteString(value);
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Compare"/>
    public override bool Compare(ref string value, ref string other)
    {
        return value == other;
    }
}