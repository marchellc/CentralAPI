using System.Net;

using LabExtended.API;
using NetworkLib;

namespace CentralAPI.ClientPlugin.Punishments.Objects;

/// <summary>
/// Contains data about a player of a punishment.
/// </summary>
public class PunishmentPlayer
{
    /// <summary>
    /// Gets the information for the server player.
    /// </summary>
    public static PunishmentPlayer Server { get; } = new()
    {
        Id = "server",
        Name = "Server",
        
        Ranks = ["server"],
        
        Address = IPAddress.Loopback
    };
    
    /// <summary>
    /// Gets or sets the player's user ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the player's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets the array of ranks the player had at the time.
    /// </summary>
    public string[] Ranks { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// Gets or sets the player's IP.
    /// </summary>
    public IPAddress Address { get; set; } = IPAddress.None;

    /// <summary>
    /// Writes the player data.
    /// </summary>
    /// <param name="writer">The target writer.</param>
    public void Write(NetworkWriter writer)
    {
        writer.WriteString(Id);
        writer.WriteString(Name);
        
        writer.WriteIpAddress(Address);
        
        writer.WriteCollection(Ranks, (_, str) => writer.WriteString(str));
    }

    /// <summary>
    /// Reads the player data.
    /// </summary>
    /// <param name="reader">The target reader.</param>
    public void Read(NetworkReader reader)
    {
        Id = reader.ReadString();
        Name = reader.ReadString();

        Address = reader.ReadIpAddress();
        
        Ranks = reader.ReadArray(_ => reader.ReadString());
    }

    /// <summary>
    /// Attempts to retrieve the connected player instance.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>true if the player was found</returns>
    public bool TryGetPlayer(out ExPlayer player)
    {
        if (string.Equals(Id, "server"))
        {
            player = ExPlayer.Host;
            return true;
        }

        return ExPlayer.TryGetByUserId(Id, out player);
    }
    
    /// <summary>
    /// Converts a connected player to a database representation.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <returns>The converted player.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static PunishmentPlayer FromPlayer(ExPlayer player)
    {
        if (player?.ReferenceHub == null)
            throw new ArgumentNullException(nameof(player));
        
        if (player.IsHost)
            return Server;

        var info = new PunishmentPlayer()
        {
            Id = player.UserId,
            Name = player.Nickname,
            
            Address = IPAddress.Parse(player.IpAddress)
        };
        
        if (!string.IsNullOrWhiteSpace(player.PermissionsGroupName))
            info.Ranks = [player.PermissionsGroupName];

        return info;
    }

    /// <summary>
    /// Creates a <see cref="PunishmentPlayer"/> instance with custom data.
    /// </summary>
    /// <param name="id">The player's ID.</param>
    /// <param name="name">The player's name.</param>
    /// <param name="address">The player's IP address.</param>
    /// <param name="ranks">The player's ranks.</param>
    /// <returns>The created instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static PunishmentPlayer FromCustom(string id, string name, IPAddress address, params string[] ranks)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentNullException(nameof(id));
        
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        
        if (address == null)
            throw new ArgumentNullException(nameof(address));
        
        return new PunishmentPlayer()
        {
            Id = id,
            Name = name,
            Address = address,
            Ranks = ranks
        };
    }
}