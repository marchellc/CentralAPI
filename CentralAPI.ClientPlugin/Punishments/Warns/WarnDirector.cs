using CentralAPI.ClientPlugin.Core;
using CentralAPI.ClientPlugin.Network;

using CentralAPI.ClientPlugin.Punishments.Objects;
using CentralAPI.ClientPlugin.Punishments.Objects.Logs;

using CentralAPI.SharedLib;
using CentralAPI.SharedLib.Punishments.Warns;

using LabExtended.API;

using LabExtended.Core;
using LabExtended.Events;
using LabExtended.Utilities;
using LabExtended.Extensions; 

using NorthwoodLib.Pools;

namespace CentralAPI.ClientPlugin.Punishments.Warns;

/// <summary>
/// Manages issued warns.
/// </summary>
public class WarnDirector : PunishmentDirector
{
    /// <summary>
    /// Gets the active warn director instance.
    /// </summary>
    public static WarnDirector Singleton { get; } = new();

    private int transactionId = 0;
    private Dictionary<int, KeyValuePair<WarnInfo, Action<WarnInfo>?>> transactions = new();

    /// <summary>
    /// Gets called once all warns are downloaded.
    /// </summary>
    public event Action? Downloaded;
    
    /// <summary>
    /// Gets called once a warn is issued.
    /// </summary>
    public event Action<WarnInfo>? Issued;

    /// <summary>
    /// Gets called once a warn is removed.
    /// </summary>
    public event Action<WarnInfo>? Removed;

    /// <summary>
    /// Gets called when a new warn is received (this also applies to each warn in the initial download).
    /// </summary>
    public event Action<WarnInfo>? Received;

    /// <summary>
    /// Gets a list of all active warns.
    /// </summary>
    public Dictionary<ulong, WarnInfo> ActiveWarns { get; } = new();
    
    /// <summary>
    /// Gets a list of all removed warns.
    /// </summary>
    public Dictionary<ulong, WarnInfo> RemovedWarns { get; } = new();
    
    private WarnDirector() { }

    /// <summary>
    /// Attempts to remove a warn.
    /// </summary>
    /// <param name="warn">The warn to remove.</param>
    /// <param name="removedBy">The player removing the warn.</param>
    /// <param name="removalReason">The reason of the warn's removal.</param>
    /// <param name="onConfirmed">The delegate to invoke once the removal is confirmed.</param>
    /// <returns>true if the removal request was sent</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool TryRemove(WarnInfo warn, PunishmentPlayer removedBy, string removalReason,
        Action<WarnInfo>? onConfirmed = null)
    {
        if (warn is null)
            throw new ArgumentNullException(nameof(warn));
        
        if (removedBy is null)
            throw new ArgumentNullException(nameof(removedBy));
        
        if (string.IsNullOrEmpty(removalReason))
            throw new ArgumentNullException(nameof(removalReason));
        
        CheckReady();

        if (!ActiveWarns.ContainsKey(warn.Id) || warn.Time.IsExpired)
            return false;

        var transaction = transactionId++;

        warn.Time.IsExpired = false;
        
        warn.Logs.Add(new DurationUpdateLog()
        {
            IsDirector = false,
            
            Server = CentralPlugin.Alias,
            
            Time = DateTime.UtcNow,
            
            Creator = removedBy,
            Reason = removalReason,
            
            NewIsExpired = true,
            PreviousIsExpired = false
        });

        ActiveWarns.Remove(warn.Id);
        RemovedWarns.Add(warn.Id, warn);
        
        NetworkClient.Scp.Send(new UpdateWarnMessage(SharedLibrary.WriteAction(warn.Write), transaction, warn.Id, true));

        transactions.Add(transactionId, new(warn, _ =>
        {
            onConfirmed?.InvokeSafe(warn);
        }));
        
        return true;
    }

    /// <summary>
    /// Issues a new warn.
    /// </summary>
    /// <param name="issuer">The warn issuer.</param>
    /// <param name="target">The warn target.</param>
    /// <param name="reason">The warn reason.</param>
    /// <param name="onConfirmed">The delegate to invoke once the warn is confirmed by the server.</param>
    /// <returns>true if the warn was issued</returns>
    public void TryIssue(PunishmentPlayer issuer, PunishmentPlayer target, string reason, Action<WarnInfo>? onConfirmed = null)
    {
        if (issuer is null)
            throw new ArgumentNullException(nameof(issuer));

        if (target is null)
            throw new ArgumentNullException(nameof(target));
        
        if (string.IsNullOrEmpty(reason))
            throw new ArgumentNullException(nameof(reason));
        
        CheckReady();

        var time = DateTime.UtcNow;
        var warn = new WarnInfo();
        var transaction = transactionId++;

        warn.Server = CentralPlugin.Alias;
        
        warn.Reason = reason;

        warn.Issuer = issuer;
        warn.Target = target;
        
        warn.Time.IsPermanent = true;
        
        warn.Time.UtcIssued = time;
        warn.Time.UtcStart = time;
        
        transactions.Add(transaction, new(warn, _ =>
        {
            if (warn.Target.TryAction(ply => DisplayWarn(ply, warn)))
            {
                warn.IsDisplayed = true;
                
                NetworkClient.Scp.Send(new UpdateWarnMessage(SharedLibrary.WriteAction(warn.Write), 0, warn.Id, false));
            }
            
            onConfirmed?.InvokeSafe(warn);
        }));
    }

    /// <summary>
    /// Attempts to get a warn matching a predicate.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <param name="warnInfo">The found warn.</param>
    /// <returns>true if a warn was found</returns>
    public bool TryGet(Predicate<WarnInfo> predicate, out WarnInfo warnInfo)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));
        
        CheckReady();

        foreach (var pair in ActiveWarns)
        {
            if (predicate(pair.Value))
            {
                warnInfo = pair.Value;
                return true;
            }
        }

        foreach (var pair in RemovedWarns)
        {
            if (predicate(pair.Value))
            {
                warnInfo = pair.Value;
                return true;
            }
        }

        warnInfo = null;
        return false;
    }
    
    /// <inheritdoc cref="PunishmentDirector.Load"/>>
    public override void Load()
    {
        ExPlayerEvents.Verified += OnVerified;
    }

    /// <inheritdoc cref="PunishmentDirector.Unload"/>>
    public override void Unload()
    {
        ExPlayerEvents.Verified -= OnVerified;
    }

    /// <inheritdoc cref="PunishmentDirector.Download"/>>
    public override void Download()
    {
        NetworkClient.Scp.HandleMessage<ConfirmedWarnMessage>(OnConfirmed);
        NetworkClient.Scp.HandleMessage<WarnPackageMessage>(OnPackage);
        NetworkClient.Scp.HandleMessage<UpdateWarnMessage>(OnUpdate);
        
        NetworkClient.Scp.Send(new WarnDownloadMessage());
    }
    
    /// <inheritdoc cref="PunishmentDirector.Disconnect"/>>
    public override void Disconnect()
    {
        transactionId = 0;
        transactions.Clear();
        
        ActiveWarns.Clear();
        RemovedWarns.Clear();
    }

    // TODO: actually display the warn lmao
    private void DisplayWarn(ExPlayer player, WarnInfo warnInfo)
    {
        
    }

    private void OnVerified(ExPlayer player)
    {
        var userId = player.UserId;
        var activeUserIdWarns = ListPool<WarnInfo>.Shared.Rent();

        Task.Run(() =>
        {
            while (TryGet(x => x.Target.Id == userId && !x.IsDisplayed, out var warn)
                   && !activeUserIdWarns.Contains(warn))
            {
                activeUserIdWarns.Add(warn);
            }
        }).ContinueWithOnMain(_ =>
        {
            foreach (var warn in activeUserIdWarns)
            {
                DisplayWarn(player, warn);

                warn.IsDisplayed = true;
                
                NetworkClient.Scp.Send(new UpdateWarnMessage(SharedLibrary.WriteAction(warn.Write), 0, warn.Id, false));
            }
            
            ListPool<WarnInfo>.Shared.Return(activeUserIdWarns);
        });
    }

    private bool OnUpdate(UpdateWarnMessage updateWarnMessage)
    {
        if (ActiveWarns.TryGetValue(updateWarnMessage.WarnId, out var updatedWarn))
        {
            updateWarnMessage.Data.ReadAction(updatedWarn.Read);

            if (updatedWarn.Time.IsExpired)
            {
                ActiveWarns.Remove(updateWarnMessage.WarnId);
                RemovedWarns.Add(updateWarnMessage.WarnId, updatedWarn);
                
                Removed?.InvokeSafe(updatedWarn);
            }
        }
        else if (RemovedWarns.TryGetValue(updateWarnMessage.WarnId, out updatedWarn))
        {
            updateWarnMessage.Data.ReadAction(updatedWarn.Read);

            if (!updatedWarn.Time.IsExpired)
            {
                RemovedWarns.Remove(updateWarnMessage.WarnId);
                ActiveWarns.Add(updateWarnMessage.WarnId, updatedWarn);
                
                Issued?.InvokeSafe(updatedWarn);
            }
        }
        else
        {
            updatedWarn = new();
            updateWarnMessage.Data.ReadAction(updatedWarn.Read);

            if (updatedWarn.Time.IsExpired)
            {
                RemovedWarns.Add(updatedWarn.Id, updatedWarn);
                Removed?.InvokeSafe(updatedWarn);
            }
            else
            {
                ActiveWarns.Add(updatedWarn.Id, updatedWarn);
                Issued?.InvokeSafe(updatedWarn);
            }
            
            Received?.InvokeSafe(updatedWarn);
        }

        return true;
    }

    private bool OnPackage(WarnPackageMessage warnPackageMessage)
    {
        warnPackageMessage.Data.ReadAction(reader =>
        {
            var count = reader.ReadInt();

            for (var i = 0; i < count; i++)
            {
                var warn = new WarnInfo();
                
                warn.Read(reader);

                if (warn.Time.IsExpired)
                    RemovedWarns.Add(warn.Id, warn);
                else
                    ActiveWarns.Add(warn.Id, warn);
                
                Received?.InvokeSafe(warn);
            }

            IsDownloaded = true;
            
            Downloaded?.InvokeSafe();
        });
        
        return true;
    }

    private bool OnConfirmed(ConfirmedWarnMessage confirmedWarnMessage)
    {
        if (transactions.TryGetValue(confirmedWarnMessage.TransactionId, out var transaction))
        {
            transactions.Remove(confirmedWarnMessage.TransactionId);

            if (confirmedWarnMessage.IsRemoval)
            {
                Removed?.InvokeSafe(transaction.Key);
                
                transaction.Value?.InvokeSafe(transaction.Key);
            }
            else
            {
                transaction.Key.Id = confirmedWarnMessage.Id;

                ActiveWarns.Add(confirmedWarnMessage.Id, transaction.Key);

                Issued?.InvokeSafe(transaction.Key);
                
                transaction.Value?.InvokeSafe(transaction.Key);
            }
        }
        else
        {
            ApiLog.Warn("Warn Director", $"Received a warn confirmation with an unknown transaction ID! (&3{confirmedWarnMessage.TransactionId}&r)");
        }

        return true;
    }
}