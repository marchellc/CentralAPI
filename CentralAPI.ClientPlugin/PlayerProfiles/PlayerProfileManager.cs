#define ENABLE_EXAMPLE_PROFILE_PROPERTIES
using System.Net;

using CentralAPI.ClientPlugin.Databases;
using CentralAPI.ClientPlugin.Network;

using CentralAPI.ClientPlugin.PlayerProfiles.Internal;
using CentralAPI.ClientPlugin.PlayerProfiles.Properties.Examples;

using CentralAPI.SharedLib;
using CentralAPI.SharedLib.PlayerProfiles;

using LabExtended.API;

using LabExtended.Core;
using LabExtended.Events;
using LabExtended.Extensions;

using LabExtended.Utilities.Update;

using NetworkLib.Enums;

namespace CentralAPI.ClientPlugin.PlayerProfiles;

/// <summary>
/// Manages player profiles.
/// </summary>
public static class PlayerProfileManager
{
    private static Queue<PlayerProfileInstance> dirtyProfiles = new();
    
    /// <summary>
    /// Gets called once a new profile is received from the server.
    /// </summary>
    public static event Action<PlayerProfileInstance>? Received;

    /// <summary>
    /// Gets called once a profile is removed (not deleted, removed, mostly due to a disconnect).
    /// </summary>
    public static event Action<PlayerProfileInstance>? Removed;

    /// <summary>
    /// Gets called once a player with a valid (or newly created) profile joins the server.
    /// </summary>
    public static event Action<PlayerProfileInstance>? Joined;

    /// <summary>
    /// Gets called once a player with a valid profile leaves the server.
    /// </summary>
    public static event Action<PlayerProfileInstance>? Left;

    /// <summary>
    /// Gets called once a profile's property is modified.
    /// </summary>
    public static event Action<PlayerProfileInstance, PlayerProfilePropertyBase>? PropertyModified;

    /// <summary>
    /// Gets called once a profile is modified.
    /// </summary>
    public static event Action<PlayerProfileInstance>? ProfileModified; 
    
    /// <summary>
    /// A list of registered property types.
    /// </summary>
    public static Dictionary<string, PlayerProfilePropertyInfo> Properties { get; } = new();

    /// <summary>
    /// A list of received player profiles.
    /// </summary>
    public static Dictionary<string, PlayerProfileInstance> Profiles { get; } = new();

    /// <summary>
    /// Registers a property.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    public static void AddProperty<T>(string name) where T : PlayerProfilePropertyBase, new()
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        if (!Properties.ContainsKey(name))
        {
            Properties.Add(name, new(name, typeof(T)));
        }
    }

    /// <summary>
    /// Deletes a profile.
    /// </summary>
    /// <param name="userId">The ID of the profile to delete.</param>
    /// <returns>true if the profile was deleted</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool DeleteProfile(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentNullException(nameof(userId));

        if (Profiles.TryGetValue(userId, out var profile))
        {
            Profiles.Remove(userId);
            
            Removed?.InvokeSafe(profile);
                
            profile.Destroy();
                
            NetworkClient.Scp.Send(new PlayerProfileUpdateMessage(userId, PlayerProfileUpdateType.Delete, null));
                
            ApiLog.Debug("Player Profile Manager", $"Deleted profile instance for {userId}");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Attempts to find a profile for a specific user ID.
    /// </summary>
    /// <param name="userId">The target user ID.</param>
    /// <param name="profile">The retrieved profile.</param>
    /// <returns>true if the profile was found</returns>
    public static bool TryGetProfile(string userId, out PlayerProfileInstance profile)
    {
        return Profiles.TryGetValue(userId, out profile);
    }
    
    /// <summary>
    /// Gets a profile.
    /// </summary>
    /// <param name="userId">The profile user ID.</param>
    /// <returns>The retrieved profile instance.</returns>
    /// <exception cref="KeyNotFoundException">Profile does not exist.</exception>
    public static PlayerProfileInstance GetProfile(string userId)
    {
        if (Profiles.TryGetValue(userId, out var profile))
            return profile;
        
        throw new KeyNotFoundException($"Profile {userId} not found");
    }

    /// <summary>
    /// Gets or creates a new player profile for a specific user ID.
    /// </summary>
    /// <param name="userId">The target user ID.</param>
    /// <returns>The retrieved or created profile instance.</returns>
    /// <exception cref="ArgumentNullException">userId is null or empty</exception>
    /// <exception cref="InvalidOperationException">Tried to create a profile for a profile with Do Not Track enabled</exception>
    /// <exception cref="Exception">Tried to create a profile for a profile that is not currently on the server.</exception>
    public static PlayerProfileInstance GetOrCreate(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentNullException(nameof(userId));
        
        if (ExPlayer.TryGetByUserId(userId, out var player) && player.DoNotTrack)
            throw new InvalidOperationException("Cannot create a profile for a player with active Do Not Track!");
        
        if (Profiles.TryGetValue(userId, out var profile))
            return profile;

        if (player is null)
            throw new Exception($"Cannot create a profile for an offline player.");

        profile = PlayerProfileInstance.CreateNew(player);
        
        Profiles.Add(player.UserId, profile);
        
        Received?.InvokeSafe(profile);
        Joined?.InvokeSafe(profile);

        return profile;
    }

    private static void OnVerified(ExPlayer player)
    {
        if (NetworkClient.Scp is null)
        {
            ApiLog.Debug("Player Profile Manager", "Player joined while the client is disconnected");
            return;
        }
        
        ApiLog.Debug("Player Profile Manager", $"Handling join: {player.UserId}");
        
        if (Profiles.TryGetValue(player.UserId, out var profile))
        {
            ApiLog.Debug("Player Profile Manager", "Found profile instance");
            
            if (player.DoNotTrack)
            {
                ApiLog.Debug("Player Profile Manager", $"Player {player.UserId} has Do Not Track active, deleting ..");
                
                Removed?.InvokeSafe(profile);
                
                Profiles.Remove(player.UserId);
                
                profile.Destroy();
                
                NetworkClient.Scp.Send(new PlayerProfileUpdateMessage(player.UserId, PlayerProfileUpdateType.Delete, null));
                
                ApiLog.Debug("Player Profile Manager", $"Deleted profile instance for {player.UserId}");
            }
            else
            {
                ApiLog.Debug("Player Profile Manager", $"Updating profile data for player {player.UserId}");
                
                profile.Player = player;
                profile.ActivityTimestamp = DateTime.UtcNow;

                if (profile.UsernameCache.LastValue?.Length < 1 || profile.UsernameCache.LastValue != player.Nickname)
                {
                    profile.UsernameCache.SetNew(player.Nickname);
                    
                    ApiLog.Debug("Player Profile Manager", $"Updated nickname cache for {player.UserId} ({player.Nickname})");
                }

                if (IPAddress.TryParse(player.IpAddress, out var ip) &&
                    (profile.AddressCache.LastValue is null || !Equals(profile.AddressCache.LastValue, ip)))
                {
                    profile.AddressCache.SetNew(ip);
                    
                    ApiLog.Debug("Player Profile Manager", $"Updated IP cache for {player.UserId} ({ip})");
                }
                
                Joined?.InvokeSafe(profile);
            }
        }
        else
        {
            ApiLog.Debug("Player Profile Manager", $"A profile for player {player.UserId} does not exist");
            
            if (player.DoNotTrack)
            {
                ApiLog.Debug("Player Profile Manager", $"Player {player.UserId} has Do Not Track enabled, not adding a profile");
                return;
            }
            
            ApiLog.Debug("Player Profile Manager", $"Adding a new profile for {player.UserId}");

            profile = PlayerProfileInstance.CreateNew(player);
            
            Profiles.Add(player.UserId, profile);
            
            ApiLog.Debug("Player Profile Manager", $"Added new profile for {player.UserId}");
            
            NetworkClient.Scp.Send(new PlayerProfileUpdateMessage(player.UserId, PlayerProfileUpdateType.Create, SharedLibrary.WriteAction(x =>
            {
                x.WriteDate(profile.CreationTimestamp);
                x.WriteDate(profile.ActivityTimestamp);
                
                x.WriteString(player.Nickname);
                x.WriteIpAddress(IPAddress.Parse(player.IpAddress));
                
                x.WriteInt(profile.Properties.Count);

                foreach (var property in profile.Properties)
                {
                    x.WriteString(property.Key);
                    x.WriteBytes(SharedLibrary.WriteAction(property.Value.Write));
                }
            })));

            profile.IsDirty = false;
            
            Received?.InvokeSafe(profile);
            Joined?.InvokeSafe(profile);
        }
    }

    private static void OnLeft(ExPlayer player)
    {
        if (Profiles.TryGetValue(player.UserId, out var profile))
        {
            foreach (var property in profile.Properties)
                property.Value.OnLeft(player);
            
            Left?.InvokeSafe(profile);
            
            profile.Player = null;
        }
    }

    private static bool OnUpdateMessage(PlayerProfileUpdateMessage updateMessage)
    {
        ApiLog.Debug("Player Profile Manager", $"Received update message for profile {updateMessage.UserId}: {updateMessage.Type} ({updateMessage.Data?.Length ?? -1})");
        
        if (updateMessage.Type is PlayerProfileUpdateType.Create)
        {
            ApiLog.Debug("Player Profile Manager", "Creating a new profile");
            
            if (!Profiles.TryGetValue(updateMessage.UserId, out var profile))
            {
                profile = new();
                profile.UserId = updateMessage.UserId;

                updateMessage.Data.ReadAction(reader =>
                {
                    profile.CreationTimestamp = reader.ReadDate();
                    profile.ActivityTimestamp = reader.ReadDate();

                    profile.UsernameCache.SetNew(reader.ReadString());
                    profile.AddressCache.SetNew(reader.ReadIpAddress());

                    var propertyCount = reader.ReadInt();

                    if (propertyCount > 0)
                    {
                        for (var i = 0; i < propertyCount; i++)
                        {
                            var propertyName = reader.ReadString();
                            var propertyValue = reader.ReadBytes();

                            if (!Properties.TryGetValue(propertyName, out var propertyInfo))
                            {
                                ApiLog.Warn("Player Profile Manager", $"Could not find property &3{propertyName}&r!");
                                continue;
                            }

                            var property = propertyInfo.Create();

                            property.Profile = profile;
                            property.Initialize();
                            
                            propertyValue.ReadAction(propertyReader => property.Read(propertyReader, true));

                            profile.Properties.Add(propertyName, property);
                        }
                    }
                });

                Profiles.Add(updateMessage.UserId, profile);

                if (ExPlayer.TryGetByUserId(updateMessage.UserId, out var player))
                {
                    profile.Player = player;
                    
                    foreach (var property in profile.Properties)
                        property.Value.OnJoined(player);
                    
                    Joined?.InvokeSafe(profile);
                }
                
                Received?.InvokeSafe(profile);
                
                ApiLog.Debug("Player Profile Manager", "Finished processing profile");
            }
        }
        else if (updateMessage.Type is PlayerProfileUpdateType.Delete)
        {
            ApiLog.Debug("Player Profile Manager", $"Deleting profile {updateMessage.UserId}");
            
            if (Profiles.TryGetValue(updateMessage.UserId, out var profile))
            {
                Profiles.Remove(updateMessage.UserId);
                
                Removed?.InvokeSafe(profile);
                
                profile.Destroy();
            }
            
            ApiLog.Debug("Player Profile Manager", $"Deleted profile");
        }
        else if (Profiles.TryGetValue(updateMessage.UserId, out var profile))
        {
            ApiLog.Debug("Player Profile Manager", $"Updating profile {updateMessage.UserId}");
            
            profile.HandleUpdate(updateMessage);
        }
        
        return true;
    }
    
    private static bool OnPackageMessage(PlayerProfilePackageMessage packageMessage)
    {
        ApiLog.Debug("Player Profile Manager", $"Received profile package ({packageMessage.Data.Length})");
        
        OnDisconnect(DisconnectReason.Undefined);
        
        packageMessage.Data.ReadAction(reader =>
        {
            var profileCount = reader.ReadInt();

            for (var i = 0; i < profileCount; i++)
            {
                var profile = new PlayerProfileInstance();
                
                profile.UserId = reader.ReadString();

                profile.CreationTimestamp = reader.ReadDate();
                profile.ActivityTimestamp = reader.ReadDate();
                
                profile.AddressCache.Read(reader, () => reader.ReadIpAddress());
                profile.UsernameCache.Read(reader, () => reader.ReadString());

                var propertyCount = reader.ReadInt();

                for (var x = 0; x < propertyCount; x++)
                {
                    var propertyName = reader.ReadString();
                    var propertyBytes = reader.ReadBytes();

                    if (!Properties.TryGetValue(propertyName, out var propertyInfo))
                    {
                        ApiLog.Warn("Player Profile Manager", $"Property &3{propertyName}&r has not been registered!");
                        continue;
                    }
                    
                    var property = propertyInfo.Create();

                    property.Profile = profile;
                    property.Initialize();
                    
                    propertyBytes.ReadAction(reader => property.Read(reader, true));
                    
                    profile.Properties.Add(propertyName, property);
                }
                
                Profiles.Add(profile.UserId, profile);
                
                foreach (var registeredProperty in Properties)
                {
                    if (!profile.Properties.ContainsKey(registeredProperty.Key))
                    {
                        var property = registeredProperty.Value.Create();

                        property.Profile = profile;
                        property.Initialize();
                        
                        profile.Properties.Add(registeredProperty.Key, property);

                        property.IsDirty = true;
                    }
                }
                
                Received?.InvokeSafe(profile);

                if (ExPlayer.TryGetByUserId(profile.UserId, out var player))
                {
                    profile.Player = player;
                    profile.ActivityTimestamp = DateTime.UtcNow;
                    
                    foreach (var property in profile.Properties)
                        property.Value.OnJoined(player);
                    
                    Joined?.Invoke(profile);
                }
            }
        });
        
        return true;
    }

    private static void OnDisconnect(DisconnectReason _)
    {
        dirtyProfiles.Clear();
        
        foreach (var profile in Profiles)
        {
            Removed?.InvokeSafe(profile.Value);
            
            profile.Value.Destroy();
        }
        
        Profiles.Clear();
    }
    
    private static void OnReady()
    {
        NetworkClient.Scp.HandleMessage<PlayerProfilePackageMessage>(OnPackageMessage);
        NetworkClient.Scp.HandleMessage<PlayerProfileUpdateMessage>(OnUpdateMessage);
    }

    private static void OnDownloaded()
        => NetworkClient.Scp.Send(new PlayerProfileDownloadMessage());
    
    internal static void OnPropertyModified(PlayerProfilePropertyBase property)
        => PropertyModified?.InvokeSafe(property.Profile, property);

    internal static void OnProfileModified(PlayerProfileInstance instance)
        => ProfileModified?.InvokeSafe(instance);

    internal static void SetDirty(PlayerProfileInstance instance)
    {
        if (instance != null && !dirtyProfiles.Contains(instance))
        {
            dirtyProfiles.Enqueue(instance);
        }
    }
    
    internal static void Init()
    {
        NetworkClient.Ready += OnReady;
        NetworkClient.Disconnected += OnDisconnect;

        ExPlayerEvents.Verified += OnVerified;
        ExPlayerEvents.Left += OnLeft;

        DatabaseDirector.Downloaded += OnDownloaded;
        
        PlayerUpdateHelper.OnUpdate += Update;
        
#if ENABLE_EXAMPLE_PROFILE_PROPERTIES
        AddProperty<JoinCountProperty>("JoinCount");
#endif
    }

    private static void Update()
    {
        try
        {
            while (NetworkClient.Scp != null && dirtyProfiles.TryDequeue(out var profile))
            {
                ApiLog.Debug("Player Profile Manager", $"Processing dirty profile {profile.UserId} ({profile.DirtyFlags} - {profile.DirtyProperties.Count})");

                var flags = profile.DirtyFlags;
                
                if (flags == PlayerProfileUpdateType.None || !profile.IsDirty)
                    continue;
                
                var data = SharedLibrary.WriteAction(writer =>
                {
                    if ((profile.DirtyFlags & PlayerProfileUpdateType.Activity) == PlayerProfileUpdateType.Activity)
                    {
                        writer.WriteDate(profile.ActivityTimestamp);
                    }

                    if ((profile.DirtyFlags & PlayerProfileUpdateType.Address) == PlayerProfileUpdateType.Address)
                    {
                        writer.WriteDate(profile.AddressCache.LastTimestamp);
                        writer.WriteIpAddress(profile.AddressCache.LastValue);
                    }

                    if ((profile.DirtyFlags & PlayerProfileUpdateType.Username) == PlayerProfileUpdateType.Username)
                    {
                        writer.WriteDate(profile.UsernameCache.LastTimestamp);
                        writer.WriteString(profile.UsernameCache.LastValue);
                    }

                    if ((profile.DirtyFlags & PlayerProfileUpdateType.Property) == PlayerProfileUpdateType.Property
                        && profile.DirtyProperties.Count > 0)
                    {
                        writer.WriteInt(profile.DirtyProperties.Count);

                        for (var i = 0; i < profile.DirtyProperties.Count; i++)
                        {
                            var property = profile.DirtyProperties[i];
                            
                            writer.WriteString(property.Name);
                            writer.WriteBytes(SharedLibrary.WriteAction(property.Write));
                        }
                    }

                    profile.IsDirty = false;
                });
                
                NetworkClient.Scp.Send(new PlayerProfileUpdateMessage(profile.UserId, flags, data));
            }
        }
        catch
        {
            // ignored
        }
    }
}