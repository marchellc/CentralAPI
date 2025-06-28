using CentralAPI.ClientPlugin.Core;
using CentralAPI.ClientPlugin.Punishments.Objects;

using LabExtended.Extensions;

namespace CentralAPI.ClientPlugin.Punishments.Warns;

/// <summary>
/// Manages warn punishments.
/// </summary>
public class WarnPunishmentDirector : PunishmentDirector<WarnPunishmentInfo>
{
    /// <summary>
    /// Gets the ID of the index key for warns.
    /// </summary>
    public const string IndexID = "activeWarn";
    
    /// <summary>
    /// Gets the warn punishment director singleton.
    /// </summary>
    public static WarnPunishmentDirector Singleton { get; } = new();

    /// <summary>
    /// Gets called once a warn is removed.
    /// </summary>
    public static event Action<WarnPunishmentInfo, PunishmentPlayer, string?> Removed;
    
    /// <inheritdoc cref="PunishmentDirector{TInfo}.TableID"/>
    public override byte TableID => CentralPlugin.Warns.TableId;
    
    /// <inheritdoc cref="PunishmentDirector{TInfo}.ActiveCollectionID"/>
    public override byte ActiveCollectionID => CentralPlugin.Warns.ActiveCollectionId;
    
    /// <inheritdoc cref="PunishmentDirector{TInfo}.ExpiredCollectionID"/>
    public override byte ExpiredCollectionID => CentralPlugin.Warns.ExpiredCollectionId;

    /// <summary>
    /// Gets the next punishment ID.
    /// </summary>
    public ulong NextId => PunishmentID.GetNext(IndexID);
    
    /// <summary>
    /// Attempts to remove an active warn.
    /// </summary>
    /// <param name="warnId">The ID of the warn.</param>
    /// <param name="removingPlayer">The player who is removing the warn.</param>
    /// <param name="note">A custom note of the removal.</param>
    /// <returns>true if the warn was removed</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool TryRemove(ulong warnId, PunishmentPlayer removingPlayer, string? note = null)
    {
        if (removingPlayer is null)
            throw new ArgumentNullException(nameof(removingPlayer));
        
        var stringWarnId = warnId.ToString();

        if (!ActivePunishments.TryGet(stringWarnId, out var warnInfo))
            return false;

        warnInfo.Duration.IsExpired = true;
        
        warnInfo.Updates.Add(new()
        {
            Type = PunishmentInfo.ReservedUpdateTypeOffset + 1,
            NewValue = note,
            Player = removingPlayer,
            Time = DateTime.UtcNow
        });
        
        ActivePunishments.Remove(stringWarnId);
        
        ExpiredPunishments.UpdateOrAdd(stringWarnId, _ => false, (ref info, isNew) =>
        {
            info = warnInfo;
        });

        Removed?.InvokeSafe(warnInfo, removingPlayer, note);
        return true;
    }
    
    /// <summary>
    /// Attempts to get an expired warn by it's ID.
    /// </summary>
    /// <param name="warnId">The ID of the warn.</param>
    /// <param name="warnInfo">The found warn information.</param>
    /// <returns>true if the warn was found</returns>
    public bool TryGetExpired(ulong warnId, out WarnPunishmentInfo warnInfo)
    {
        CheckInitialized();
        
        var stringWarnId = warnId.ToString();
        return ExpiredPunishments.TryGet(stringWarnId, out warnInfo);
    }
    
    /// <summary>
    /// Attempts to get an active warn by it's ID.
    /// </summary>
    /// <param name="warnId">The ID of the warn.</param>
    /// <param name="warnInfo">The found warn information.</param>
    /// <returns>true if the warn was found</returns>
    public bool TryGetActive(ulong warnId, out WarnPunishmentInfo warnInfo)
    {
        CheckInitialized();
        
        var stringWarnId = warnId.ToString();
        return ActivePunishments.TryGet(stringWarnId, out warnInfo);
    }

    /// <summary>
    /// Attempts to get an active or expired warn by it's ID.
    /// </summary>
    /// <param name="warnId">The ID of the warn.</param>
    /// <param name="warnInfo">The found warn information.</param>
    /// <returns>true if the warn was found</returns>
    public bool TryGet(ulong warnId, out WarnPunishmentInfo warnInfo)
    {
        CheckInitialized();
        
        var stringWarnId = warnId.ToString();

        if (ActivePunishments.TryGet(stringWarnId, out warnInfo))
            return true;
        
        return ExpiredPunishments.TryGet(stringWarnId, out warnInfo);
    }
}