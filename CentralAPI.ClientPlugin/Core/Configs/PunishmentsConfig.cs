using System.ComponentModel;

namespace CentralAPI.ClientPlugin.Core.Configs;

public class PunishmentsConfig
{
    [Description("Whether or not the punishment manager should be enabled.")]
    public bool Enabled { get; set; } = true;
    
    [Description("The ID of the punishments database table.")]
    public byte TableId { get; set; } = 0;
    
    [Description("The ID of the collection of expired punishments.")]
    public byte ExpiredCollectionId { get; set; } = 0;
    
    [Description("The ID of the collection of active punishments.")]
    public byte ActiveCollectionId { get; set; } = 1;
}