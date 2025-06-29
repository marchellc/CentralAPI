using LabExtended.API;
using LabExtended.Extensions;

using NetworkLib;

namespace CentralAPI.ClientPlugin.Punishments.Objects;

/// <summary>
/// Used as a record about a player.
/// </summary>
public class PunishmentPlayer
{
    /// <summary>
    /// Gets the server player info.
    /// </summary>
    public static PunishmentPlayer Server { get; } = new()
    {
        Id = "server",
        Name = "server",
        Address = "server",
        
        Ranks = ["server"]
    };
    
    /// <summary>
    /// Gets or sets the player's user ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the player's nickname.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the player's IP address.
    /// </summary>
    public string Address { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the list of ranks the player had active when creating this record.
    /// </summary>
    public string[] Ranks { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// Whether or not this record is the server player.
    /// </summary>
    public bool IsServer => 
        Id == "server" 
        && Name == "server" 
        && Address == "server" 
        && Ranks.Length == 1 
        && Ranks[0] == "server";

    /// <summary>
    /// Attempts to invoke a delegate on the player this record targets.
    /// </summary>
    /// <param name="action">The delegate to invoke.</param>
    /// <returns>true if the delegate was invoked</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool TryAction(Action<ExPlayer> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        if (!TryGet(out var player))
            return false;
        
        action.InvokeSafe(player);
        return true;
    }

    /// <summary>
    /// Attempts to find the target player.
    /// </summary>
    /// <param name="player">The found player.</param>
    /// <returns>true if the player was found</returns>
    public bool TryGet(out ExPlayer player)
    {
        if (string.IsNullOrEmpty(Id))
        {
            player = null;
            return false;
        }
        
        if (IsServer)
        {
            player = ExPlayer.Host;
            return true;
        }

        return ExPlayer.TryGetByUserId(Id, out player);
    }

    /// <summary>
    /// Writes the data.
    /// </summary>
    /// <param name="writer">The target writer.</param>
    public void Write(NetworkWriter writer)
    {
        writer.WriteString(Id);
        writer.WriteString(Name);
        writer.WriteString(Address);
        writer.WriteCollection(Ranks, (_, str) => writer.WriteString(str));
    }

    /// <summary>
    /// Reads the data.
    /// </summary>
    /// <param name="reader">The target reader.</param>
    public void Read(NetworkReader reader)
    {
        Id = reader.ReadString();
        Name = reader.ReadString();
        Address = reader.ReadString();
        Ranks = reader.ReadArray(_ => reader.ReadString());
    }

    /// <summary>
    /// Creates a new <see cref="PunishmentPlayer"/> instance containing information about a specific player.
    /// </summary>
    /// <param name="target">The target player.</param>
    /// <returns>The created instance</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static PunishmentPlayer FromPlayer(ExPlayer target)
    {
        if (target?.ReferenceHub == null)
            throw new ArgumentNullException(nameof(target));

        if (target.IsServer)
            return Server;

        if (target.IsUnverified)
            throw new Exception("Player has not been verified yet.");

        var info = new PunishmentPlayer();

        info.Id = target.UserId;
        info.Name = target.Nickname;
        info.Address = target.IpAddress;
        
        if (!string.IsNullOrEmpty(target.PermissionsGroupName))
            info.Ranks = [target.PermissionsGroupName];

        return info;
    }
}