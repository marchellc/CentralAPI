using System.Collections.Concurrent;
using System.Diagnostics;

using CentralAPI.ServerApp.Server;
using CentralAPI.SharedLib;
using CentralAPI.SharedLib.PlayerProfiles;

using CommonLib;

using NetworkLib;

namespace CentralAPI.ServerApp.PlayerProfiles;

/// <summary>
/// Manages player profiles.
/// </summary>
public static class PlayerProfileManager
{
    private static volatile ConcurrentQueue<PlayerProfileInstance> dirtyQueue = new();
    private static volatile Stopwatch saveTimer = new();
    
    /// <summary>
    /// Gets a list of all loaded player profiles.
    /// </summary>
    public static volatile ConcurrentDictionary<string, PlayerProfileInstance> Profiles = new();
    
    /// <summary>
    /// Gets the path to the root directory.
    /// </summary>
    public static volatile string RootDirectory = Path.Combine(Directory.GetCurrentDirectory(), "profiles");

    /// <summary>
    /// Creates a new profile from received network data.
    /// </summary>
    /// <param name="userId">The profile's user ID.</param>
    /// <param name="reader">The network data reader.</param>
    /// <returns>The created profile.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static PlayerProfileInstance CreateProfileFromNetwork(string userId, NetworkReader reader)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentNullException(nameof(userId));

        if (reader is null)
            throw new ArgumentNullException(nameof(reader));
        
        CommonLog.Debug("Player Profile Manager", $"Creating new profile '{userId}'");
        
        var profile = new PlayerProfileInstance();
        
        profile.UserId = userId;

        profile.ProfileDirectory = Path.Combine(RootDirectory, userId);
        profile.ProfilePath = Path.Combine(profile.ProfileDirectory, "profileData");

        profile.CreationTimestamp = reader.ReadDate();
        profile.ActivityTimestamp = reader.ReadDate();
                
        profile.UsernameCache.SetNew(reader.ReadString(), false);
        profile.AddressCache.SetNew(reader.ReadIpAddress(), false);

        var propertyCount = reader.ReadInt();

        for (var i = 0; i < propertyCount; i++)
        {
            var propertyName = reader.ReadString();
            var propertyValue = reader.ReadBytes();

            var property = new PlayerProfileProperty();

            property.Name = propertyName;
            property.RawData = propertyValue;

            property.FilePath = Path.Combine(profile.ProfileDirectory, $"{propertyName}.profileProperty");
                    
            profile.Properties.TryAdd(propertyName, property);
        }

        Profiles.TryAdd(userId, profile);
                
        SaveProfile(profile);
        return profile;
    }

    /// <summary>
    /// Loads all saved profiles.
    /// </summary>
    public static void LoadProfiles()
    {
        Profiles.Clear();
        
        CommonLog.Info("Player Profile Manager", $"Loading saved profiles ..");

        if (!Directory.Exists(RootDirectory))
        {
            Directory.CreateDirectory(RootDirectory);
        }
        else
        {
            foreach (var directory in Directory.GetDirectories(RootDirectory))
            {
                var profileId = Path.GetFileName(directory);
                var profile = new PlayerProfileInstance();

                profile.ProfileDirectory = directory;
                profile.ProfilePath = Path.Combine(directory, "profileData");

                profile.UserId = profileId;

                CommonLog.Debug("Player Profile Manager", $"Loading profile '{profileId}' (in: {directory})");

                profile.ReadProfile();

                Profiles.TryAdd(profileId, profile);

                CommonLog.Debug("Player Profile Manager", $"Saved profile {profile.UserId}!");
            }
        }

        CommonLog.Info("Player Profile Manager", $"Loaded {Profiles.Count} profile(s)");
    }

    /// <summary>
    /// Saves all loaded profiles.
    /// </summary>
    public static void SaveProfiles()
    {
        CommonLog.Info("Player Profile Manager", $"Saving {Profiles.Count} profile(s)");
        
        if (Profiles.Count == 0)
            return;
        
        if (!Directory.Exists(RootDirectory))
            Directory.CreateDirectory(RootDirectory);

        foreach (var profilePair in Profiles)
        {
            CommonLog.Debug("Player Profile Manager", $"Saving profile '{profilePair.Key}' (in: {profilePair.Value.ProfileDirectory})");
            
            if (!Directory.Exists(profilePair.Value.ProfileDirectory))
                Directory.CreateDirectory(profilePair.Value.ProfileDirectory);
            
            if (string.IsNullOrEmpty(profilePair.Value.ProfilePath))
                profilePair.Value.ProfilePath = Path.Combine(profilePair.Value.ProfileDirectory, "profileData");
            
            profilePair.Value.WriteProfile();
            
            CommonLog.Debug("Player Profile Manager", $"Saved profile '{profilePair.Key}'");
        }
    }

    /// <summary>
    /// Saves a specific profile.
    /// </summary>
    /// <param name="profile">The target profile.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void SaveProfile(PlayerProfileInstance profile)
    {
        if (profile is null)
            throw new ArgumentNullException(nameof(profile));
        
        CommonLog.Info("Player Profile Manager", $"Saving profile '{profile.UserId}' (in: {profile.ProfileDirectory})");
        
        if (!Directory.Exists(RootDirectory))
            Directory.CreateDirectory(RootDirectory);
        
        if (!Directory.Exists(profile.ProfileDirectory))
            Directory.CreateDirectory(profile.ProfileDirectory);
            
        if (string.IsNullOrEmpty(profile.ProfilePath))
            profile.ProfilePath = Path.Combine(profile.ProfileDirectory, "profileData");
            
        profile.WriteProfile();
        
        CommonLog.Info("Player Profile Manager", $"Saved profile '{profile.UserId}'");
    }

    /// <summary>
    /// Sets a profile instance as dirty.
    /// </summary>
    /// <param name="profile">The profile to set as dirty.</param>
    public static void SetDirty(PlayerProfileInstance profile)
    {
        if (profile is null)
            throw new ArgumentNullException(nameof(profile));

        if (dirtyQueue.Contains(profile))
            return;
        
        dirtyQueue.Enqueue(profile);
        
        CommonLog.Debug("Player Profile Manager", $"Profile '{profile.UserId}' is now marked as dirty.");
    }

    private static void UpdateDirty()
    {
        while (dirtyQueue.TryDequeue(out var dirtyProfile))
        {
            try
            {
                SaveProfile(dirtyProfile);
            }
            catch
            {
                // ignored
            }
        }
    }

    private static void SavePeriodically()
    {
        Task.Run(async () =>
        {
            saveTimer.Restart();
            
            while (true)
            {
                await Task.Delay(10);
                
                UpdateDirty();

                if (saveTimer.Elapsed.TotalMinutes >= 5)
                {
                    try
                    {
                        SaveProfiles();
                    }
                    catch
                    {
                        // ignored
                    }
                    
                    saveTimer.Restart();
                }
            }
        });
    }

    private static bool OnUpdate(ScpInstance scpInstance, PlayerProfileUpdateMessage message)
    {
        CommonLog.Debug("Player Profile Manager", $"Received profile update ({message.UserId}; {message.Type})");

        if (message.Type is PlayerProfileUpdateType.Create)
        {
            if (!Profiles.ContainsKey(message.UserId))
            {
                if (message.Data?.Length < 1)
                {
                    CommonLog.Warn("Player Profile Manager",
                        $"Received a creation request for profile '{message.UserId}' with invalid data!");
                    return true;
                }

                message.Data.ReadAction(reader => _ = CreateProfileFromNetwork(message.UserId, reader));
                message.SendToWhere(x => x.Connection.Id != scpInstance.Connection.Id);
                
                return true;
            }

            CommonLog.Warn("Player Profile Manager",
                $"Received a creation request for profile '{message.UserId}', but it already exists.");
            return true;
        }
        
        if (message.Type is PlayerProfileUpdateType.Delete)
        {
            if (Profiles.TryRemove(message.UserId, out var removedProfile))
            {
                var dirtyList = new List<PlayerProfileInstance>(dirtyQueue.Count);
                
                while (dirtyQueue.TryDequeue(out var dirtyProfile))
                {
                    if (dirtyProfile.UserId != message.UserId)
                    {
                        dirtyList.Add(dirtyProfile);
                    }
                }
                
                dirtyList.ForEach(p => dirtyQueue.Enqueue(p));
                dirtyList.Clear();

                try
                {
                    Directory.Delete(removedProfile.ProfileDirectory, true);
                }
                catch
                {
                    // ignored
                }
                
                message.SendToWhere(x => x.Connection.Id != scpInstance.Connection.Id);
                return true;
            }

            CommonLog.Warn("Player Profile Manager", $"Received a deletion request for a non-existent profile '{message.UserId}'");
            return true;
        }
        
        if (Profiles.TryGetValue(message.UserId, out var profile))
        {
            profile.HandleUpdate(scpInstance, message);
            return true;
        }
        
        CommonLog.Warn("Player Profile Manager", $"Received an update request for a non-existent profile '{message.UserId}'");
        return true;
    }

    private static bool OnDownload(ScpInstance scpInstance, PlayerProfileDownloadMessage _)
    {
        CommonLog.Debug("Player Profile Manager", $"Received download request from '{scpInstance.Connection.Id}'");

        var data = SharedLibrary.WriteAction(writer =>
        {
            writer.WriteInt(Profiles.Count);

            foreach (var profile in Profiles)
            {
                writer.WriteString(profile.Key);
                
                writer.WriteDate(profile.Value.CreationTimestamp);
                writer.WriteDate(profile.Value.ActivityTimestamp);
                
                profile.Value.AddressCache.Write(writer, address => writer.WriteIpAddress(address));
                profile.Value.UsernameCache.Write(writer, name => writer.WriteString(name));
                
                writer.WriteInt(profile.Value.Properties.Count);

                foreach (var property in profile.Value.Properties)
                {
                    writer.WriteString(property.Key);
                    writer.WriteBytes(property.Value.RawData);
                }
            }
        });
        
        scpInstance.Send(new PlayerProfilePackageMessage(data));
        return true;
    }

    internal static void Init()
    {
        LoadProfiles();
        SavePeriodically();
        
        ScpInstance.HandleMessage<PlayerProfileUpdateMessage>(OnUpdate);
        ScpInstance.HandleMessage<PlayerProfileDownloadMessage>(OnDownload);
    }
}