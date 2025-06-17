using NetworkLib;

namespace CentralAPI.ClientPlugin.Databases.Wrappers;

/// <summary>
/// Manages serialization of floating-point numbers.
/// </summary>
public class DatabaseFloat : DatabaseWrapper<float>
{
    /// <inheritdoc cref="DatabaseWrapper{T}.Read"/>
    public override void Read(NetworkReader reader, ref float value)
    {
        value = reader.ReadFloat();
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Write"/>
    public override void Write(NetworkWriter writer, ref float value)
    {
        writer.WriteFloat(value);
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Compare"/>
    public override bool Compare(ref float value, ref float other)
    {
        return value == other;
    }
    
    /// <inheritdoc cref="DatabaseWrapper{T}.Convert"/>
    public override void Convert(float value, out string result)
    {
        result = value.ToString();
    }
}