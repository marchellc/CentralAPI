using CentralAPI.SharedLib;

using NetworkLib;

namespace CentralAPI.ClientPlugin.Punishments.Objects;

/// <summary>
/// Contains punishment information.
/// </summary>
public abstract class PunishmentInfo
{
    /// <summary>
    /// Gets the ID of the last reserved update type (which can be used as an offset for custom types).
    /// </summary>
    public const byte ReservedUpdateTypeOffset = 1;
    
    /// <summary>
    /// Whether or not this punishment can expire.
    /// </summary>
    public abstract bool CanExpire { get; }
    
    /// <summary>
    /// Gets the ID of the punishment.
    /// </summary>
    public ulong Id { get; internal set; }

    /// <summary>
    /// Gets the issuing player.
    /// </summary>
    public PunishmentPlayer Issuer { get; } = new();

    /// <summary>
    /// Gets the target player.
    /// </summary>
    public PunishmentPlayer Target { get; } = new();

    /// <summary>
    /// Gets the duration of the punishment.
    /// </summary>
    public PunishmentDuration? Duration { get; private set; }
    
    /// <summary>
    /// Gets the reason of the punishment.
    /// </summary>
    public PunishmentReason Reason { get; } = new();

    /// <summary>
    /// Gets a list of punishment updates.
    /// </summary>
    public List<PunishmentUpdate> Updates { get; } = new();

    /// <summary>
    /// Creates a new <see cref="PunishmentInfo"/> instance.
    /// </summary>
    public PunishmentInfo()
    {
        if (CanExpire)
        {
            Duration = new();
        }
    }

    /// <summary>
    /// Reads the punishment.
    /// </summary>
    /// <param name="reader">The target reader.</param>
    public virtual void Read(NetworkReader reader)
    {
        var updateCount = reader.ReadInt();

        for (var i = 0; i < updateCount; i++)
        {
            var updateType = reader.ReadByte();

            // Duration update
            if (updateType == 0)
            {
                var update = new PunishmentUpdate();

                update.Type = 0;
                update.Time = reader.ReadDate();

                update.OriginalValue = reader.ReadTime();
                update.NewValue = reader.ReadTime();
                
                update.Player = new();
                update.Player.Read(reader);
                
                Updates.Add(update);
            }
            // Notes update
            else if (updateType == 1)
            {
                var update = new PunishmentUpdate();

                update.Type = 1;
                update.Time = reader.ReadDate();

                update.OriginalValue = reader.ReadString();
                
                update.Player = new();
                update.Player.Read(reader);
                
                Updates.Add(update);
            }
            // Custom update
            else
            {
                var update = ReadUpdate(updateType, reader.ReadDate(), reader);

                if (update != null)
                {
                    Updates.Add(update);
                }
            }
        }
    }

    /// <summary>
    /// Writes the punishment.
    /// </summary>
    /// <param name="writer">The target writer.</param>
    public virtual void Write(NetworkWriter writer)
    {
        writer.WriteInt(Updates.Count);

        for (var i = 0; i < Updates.Count; i++)
        {
            var update = Updates[i];

            writer.WriteByte(update.Type);
            writer.WriteDate(update.Time);
            
            // Duration update
            if (update.Type == 0)
            {
                writer.WriteTime((TimeSpan)update.OriginalValue);
                writer.WriteTime((TimeSpan)update.NewValue);
                
                update.Player.Write(writer);
            }
            // Notes update
            else if (update.Type == 1)
            {
                writer.WriteString(update.OriginalValue.ToString());
            }
            // Custom update
            else
            {
                WriteUpdate(update, writer);
            }
        }
    }

    /// <summary>
    /// Reads an update log.
    /// </summary>
    /// <param name="updateType">The type of the update log.</param>
    /// <param name="updateTime">The date of when the update was created.</param>
    /// <param name="reader">The target reader.</param>
    /// <returns>The read update log.</returns>
    public virtual PunishmentUpdate ReadUpdate(byte updateType, DateTime updateTime, NetworkReader reader)
    {
        return null;
    }

    /// <summary>
    /// Writes an update log.
    /// </summary>
    /// <param name="update">The update log.</param>
    /// <param name="writer">The target writer.</param>
    public virtual void WriteUpdate(PunishmentUpdate update, NetworkWriter writer)
    {
        
    }
}