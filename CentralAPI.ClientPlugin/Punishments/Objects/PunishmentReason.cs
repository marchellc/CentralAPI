using NetworkLib;

namespace CentralAPI.ClientPlugin.Punishments.Objects;

/// <summary>
/// The reason of a punishment.
/// </summary>
public class PunishmentReason
{
    /// <summary>
    /// Gets or sets the section of the rule.
    /// </summary>
    public byte RuleSection { get; set; }
    
    /// <summary>
    /// Gets or sets the number of the rule.
    /// </summary>
    public byte RuleNumber { get; set; }
    
    /// <summary>
    /// Gets or sets a custom note.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Reads the reason data.
    /// </summary>
    /// <param name="reader">The target reader.</param>
    public void Read(NetworkReader reader)
    {
        RuleSection = reader.ReadByte();
        RuleNumber = reader.ReadByte();
        
        Note = reader.ReadString();
    }

    /// <summary>
    /// Writes the reason data.
    /// </summary>
    /// <param name="writer">The target writer.</param>
    public void Write(NetworkWriter writer)
    {
        writer.WriteByte(RuleSection);
        writer.WriteByte(RuleNumber);
        
        writer.WriteString(Note ?? string.Empty);
    }
}