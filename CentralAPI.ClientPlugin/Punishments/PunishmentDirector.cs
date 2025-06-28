using CentralAPI.ClientPlugin.Core; 
using CentralAPI.ClientPlugin.Network;
using CentralAPI.ClientPlugin.Databases;

using CentralAPI.ClientPlugin.Punishments.Warns;
using CentralAPI.ClientPlugin.Punishments.Objects; 
using CentralAPI.ClientPlugin.Punishments.Wrappers;

using LabExtended.Attributes;

using NetworkLib.Enums;

namespace CentralAPI.ClientPlugin.Punishments;

/// <summary>
/// Base class for punishment directors.
/// </summary>
public abstract class PunishmentDirector<TInfo> where TInfo : PunishmentInfo
{
    private bool isInitialized;
    
    /// <summary>
    /// Gets the punishment database table.
    /// </summary>
    public DatabaseTable Table { get; private set; }
    
    /// <summary>
    /// Gets the collection of expired punishments.
    /// </summary>
    public DatabaseCollection<TInfo> ExpiredPunishments { get; private set; }
    
    /// <summary>
    /// Gets the collection of active punishments.
    /// </summary>
    public DatabaseCollection<TInfo> ActivePunishments { get; private set; }

    /// <summary>
    /// Whether or not the director is initialized.
    /// </summary>
    public bool IsInitialized => isInitialized;
    
    /// <summary>
    /// Gets the ID of the table.
    /// </summary>
    public abstract byte TableID { get; }

    /// <summary>
    /// Gets the ID of the active collection.
    /// </summary>
    public abstract byte ActiveCollectionID { get; }
    
    /// <summary>
    /// Gets the ID of the expired collection.
    /// </summary>
    public abstract byte ExpiredCollectionID { get; }
    
    /// <summary>
    /// Initializes the director.
    /// </summary>
    public virtual void Initialize()
    {
        if (!isInitialized)
        {
            Table = DatabaseDirector.GetOrAddTable(TableID);

            ActivePunishments = Table.GetOrAddCollection<TInfo>(ActiveCollectionID);
            ExpiredPunishments = Table.GetOrAddCollection<TInfo>(ExpiredCollectionID);

            isInitialized = true;
        }
    }

    /// <summary>
    /// Unloads the director.
    /// </summary>
    public virtual void Unload()
    {
        isInitialized = false;
    }

    /// <summary>
    /// Checks if the director is initialized and throws an exception if not.
    /// </summary>
    public virtual void CheckInitialized()
    {
        if (!IsInitialized)
            throw new Exception($"PunishmentDirector {GetType().Name} has not been initialized!");
    }
    
    [LoaderInitialize(1)]
    private static void OnInit()
    {
        DatabaseDirector.Downloaded += OnDownloaded;
        NetworkClient.Disconnected += OnDisconnected;
        
        DatabaseDirector.RegisterWrapper(new DatabasePunishmentInfo<WarnPunishmentInfo>(() => new()));
    }

    private static void OnDownloaded()
    {
        PunishmentID.OnDownloaded();
        
        if (CentralPlugin.Warns.Enabled)
            WarnPunishmentDirector.Singleton.Initialize();
    }

    private static void OnDisconnected(DisconnectReason _)
    {
        PunishmentID.OnDisconnected();
        
        if (CentralPlugin.Warns.Enabled && WarnPunishmentDirector.Singleton.isInitialized)
            WarnPunishmentDirector.Singleton.Unload();
    }
}