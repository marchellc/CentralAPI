using CentralAPI.ClientPlugin.Punishments.Objects.Logs;
using CentralAPI.SharedLib;

using NetworkLib;

namespace CentralAPI.ClientPlugin.Punishments.Objects;

/// <summary>
/// Contains information about a punishment.
/// </summary>
public abstract class PunishmentInfo
{
    /// <summary>
    /// Gets or sets the alias of the server the punishment was issued on.
    /// </summary>
    public string Server { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the reason of the punishment.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the ID of the punishment.
    /// </summary>
    public ulong Id { get; set; }
    
    /// <summary>
    /// Gets the time information of the punishment.
    /// </summary>
    public PunishmentTime Time { get; } = new();

    /// <summary>
    /// Gets the record of the issuing player.
    /// </summary>
    public PunishmentPlayer Issuer { get; internal set; }

    /// <summary>
    /// Gets the record of the target player.
    /// </summary>
    public PunishmentPlayer Target { get; internal set; }

    /// <summary>
    /// Contains logged punishment data updates.
    /// </summary>
    public List<PunishmentLog> Logs { get; } = new();

    /// <summary>
    /// Reads the data.
    /// </summary>
    /// <param name="reader">The target reader.</param>
    public virtual void Read(NetworkReader reader)
    {
        Issuer ??= new();
        Target ??= new();

        Id = reader.ReadULong();

        Server = reader.ReadString();
        Reason = reader.ReadString();

        Time.Read(reader);
        Issuer.Read(reader);
        Target.Read(reader);

        Logs.Clear();

        var logCount = reader.ReadInt();

        for (var i = 0; i < logCount; i++)
        {
            var logType = reader.ReadByte();
            var log = CreateLog(logType);

            if (log != null)
            {
                log.IsDirector = reader.ReadBool();
                log.Server = reader.ReadString();
                log.Time = reader.ReadDate();

                if (!log.IsDirector)
                {
                    log.Creator = new();
                    log.Creator.Read(reader);
                }
                
                log.Read(reader);
                
                Logs.Add(log);
            }
        }
    }

    /// <summary>
    /// Writes the data.
    /// </summary>
    /// <param name="writer">The target writer.</param>
    public virtual void Write(NetworkWriter writer)
    {
        writer.WriteULong(Id);
        
        writer.WriteString(Server);
        writer.WriteString(Reason);
        
        Time.Write(writer);
        Issuer.Write(writer);
        Target.Write(writer);
        
        writer.WriteInt(Logs.Count);
        
        Logs.ForEach(log =>
        {
            writer.WriteByte(log.LogType);
            writer.WriteBool(log.IsDirector);
            writer.WriteString(log.Server);
            writer.WriteDate(log.Time);
            
            if (!log.IsDirector)
                log.Creator?.Write(writer);
            
            log.Write(writer);
        });
    }

    /// <summary>
    /// Creates a log.
    /// </summary>
    /// <param name="logType">The type of the log to create.</param>
    /// <returns>The created log.</returns>
    public virtual PunishmentLog CreateLog(byte logType)
    {
        switch (logType)
        {
            case 0:
                return new ReasonUpdateLog();
            
            case 1:
                return new DurationUpdateLog();
            
            default:
                return null;
        }
    }
}