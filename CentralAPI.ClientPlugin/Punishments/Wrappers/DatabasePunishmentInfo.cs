using CentralAPI.ClientPlugin.Databases;
using CentralAPI.ClientPlugin.Punishments.Objects;

using NetworkLib;

namespace CentralAPI.ClientPlugin.Punishments.Wrappers;

/// <summary>
/// Used to serialize and deserialize <see cref="PunishmentInfo"/>
/// </summary>
public class DatabasePunishmentInfo<TInfo> : DatabaseWrapper<TInfo> where TInfo : PunishmentInfo
{
    /// <summary>
    /// Gets the constructor.
    /// </summary>
    public Func<TInfo> Constructor { get; }

    /// <summary>
    /// Creates a a new <see cref="DatabasePunishmentInfo{TInfo}"/> wrapper instance.
    /// </summary>
    /// <param name="constructor">The constructor delegate.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public DatabasePunishmentInfo(Func<TInfo> constructor)
    {
        if (constructor is null)
            throw new ArgumentNullException(nameof(constructor));
        
        Constructor = constructor;
    }
    
    /// <inheritdoc cref="DatabaseWrapper{T}.Read"/>
    public override void Read(NetworkReader reader, ref TInfo value)
    {
        value ??= Constructor();

        value.Id = reader.ReadULong();
        
        value.Issuer.Read(reader);
        value.Target.Read(reader);
        
        if (value.CanExpire && value.Duration != null)
            value.Duration.Read(reader);
        
        value.Reason.Read(reader);
        
        value.Read(reader);
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Write"/>
    public override void Write(NetworkWriter writer, ref TInfo value)
    {
        if (value != null)
        {
            writer.WriteULong(value.Id);
            
            value.Issuer.Write(writer);
            value.Target.Write(writer);
            
            if (value.CanExpire && value.Duration != null)
                value.Duration.Write(writer);
            
            value.Reason.Write(writer);
            
            value.Write(writer);
        }
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Compare"/>
    public override bool Compare(ref TInfo value, ref TInfo other)
    {
        return false;
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Convert"/>
    public override void Convert(TInfo value, out string result)
    {
        base.Convert(value, out result);
    }
}