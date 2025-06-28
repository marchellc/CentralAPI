using System.Collections.Concurrent;
using System.Net;
using CentralAPI.ServerApp.Server;
using CentralAPI.SharedLib;
using CentralAPI.SharedLib.PlayerProfiles;
using CommonLib;
using NetworkLib;
using NetworkLib.Pools;

namespace CentralAPI.ServerApp.PlayerProfiles;

/// <summary>
/// A loaded player profile instance.
/// </summary>
public class PlayerProfileInstance
{
    /// <summary>
    /// Gets the path to the profile's directory.
    /// </summary>
    public volatile string? ProfileDirectory;

    /// <summary>
    /// Gets the path to the profile's data file.
    /// </summary>
    public volatile string? ProfilePath;
    
    /// <summary>
    /// Gets the user ID of the profile's user.
    /// </summary>
    public volatile string? UserId;

    /// <summary>
    /// Gets the timestamp of the profile's creation.
    /// </summary>
    public DateTime CreationTimestamp;

    /// <summary>
    /// Gets the timestamp of the profile's last activity.
    /// </summary>
    public DateTime ActivityTimestamp;

    /// <summary>
    /// Gets the IP address of the profile's user.
    /// </summary>
    public volatile PlayerProfileCache<IPAddress> AddressCache = new();
    
    /// <summary>
    /// Gets the profile's username cache.
    /// </summary>
    public volatile PlayerProfileCache<string> UsernameCache = new();

    /// <summary>
    /// Gets all created properties.
    /// </summary>
    public volatile ConcurrentDictionary<string, PlayerProfileProperty> Properties = new();

    /// <summary>
    /// Reads all data of this profile.
    /// </summary>
    public void ReadProfile()
    {
        Properties.Clear();

        if (File.Exists(ProfilePath))
        {
            var data = File.ReadAllBytes(ProfilePath);
            
            data.ReadAction(reader =>
            {
                CreationTimestamp = reader.ReadDate();
                ActivityTimestamp = reader.ReadDate();
        
                AddressCache.Read(reader, () => reader.ReadIpAddress());
                UsernameCache.Read(reader, () => reader.ReadString());
            });
        }
        else
        {
            throw new Exception($"ProfileData file for profile {UserId} does not exist!");
        }
        
        foreach (var file in Directory.GetFiles(ProfileDirectory))
        {
            if (file.EndsWith(".profileProperty"))
            {
                var property = new PlayerProfileProperty();
                
                property.FilePath = file;
                property.Name = Path.GetFileNameWithoutExtension(file);
                property.RawData = File.ReadAllBytes(file);

                Properties.TryAdd(property.Name, property);
            }
        }
    }

    /// <summary>
    /// Writes all data.
    /// </summary>
    public void WriteProfile()
    {
        var data = SharedLibrary.WriteAction(writer =>
        {
            writer.WriteDate(CreationTimestamp);
            writer.WriteDate(ActivityTimestamp);

            AddressCache.Write(writer, ip => writer.WriteIpAddress(ip));
            UsernameCache.Write(writer, name => writer.WriteString(name));
        });
        
        File.WriteAllBytes(ProfilePath, data);

        foreach (var property in Properties)
        {
            File.WriteAllBytes(property.Value.FilePath, property.Value.RawData);
        }
    }

    internal void HandleUpdate(ScpInstance scpInstance, PlayerProfileUpdateMessage message)
    {
        var sendToOthers = false;
        var setDirty = false;
        
        CommonLog.Debug("Player Profile Manager", $"[{UserId}] Handling update: {message.Type} ({message.Data?.Length ?? -1})");

        message.Data.ReadAction(reader =>
        {
            if ((message.Type & PlayerProfileUpdateType.Activity) == PlayerProfileUpdateType.Activity)
            {
                sendToOthers = true;
                setDirty = true;

                ActivityTimestamp = reader.ReadDate();
            }

            if ((message.Type & PlayerProfileUpdateType.Address) == PlayerProfileUpdateType.Address)
            {
                sendToOthers = true;
                setDirty = true;

                if (AddressCache.LastValue != null && AddressCache.LastTimestamp.Ticks != 0 
                                                   && !AddressCache.History.ContainsKey(AddressCache.LastTimestamp))
                    AddressCache.History.TryAdd(AddressCache.LastTimestamp, AddressCache.LastValue);

                AddressCache.LastTimestamp = reader.ReadDate();
                AddressCache.LastValue = reader.ReadIpAddress();
                
                if (AddressCache.History.IsEmpty)
                    AddressCache.History.TryAdd(AddressCache.LastTimestamp, AddressCache.LastValue);
            }

            if ((message.Type & PlayerProfileUpdateType.Username) == PlayerProfileUpdateType.Username)
            {
                sendToOthers = true;
                setDirty = true;

                if (!string.IsNullOrWhiteSpace(UsernameCache.LastValue) && UsernameCache.LastTimestamp.Ticks != 0 
                                                                        && !UsernameCache.History.ContainsKey(UsernameCache.LastTimestamp))
                    UsernameCache.History.TryAdd(UsernameCache.LastTimestamp, UsernameCache.LastValue);

                UsernameCache.LastTimestamp = reader.ReadDate();
                UsernameCache.LastValue = reader.ReadString();
                
                if (UsernameCache.History.IsEmpty)
                    UsernameCache.History.TryAdd(UsernameCache.LastTimestamp, UsernameCache.LastValue);
            }

            if ((message.Type & PlayerProfileUpdateType.Property) == PlayerProfileUpdateType.Property)
            {
                sendToOthers = true;
                setDirty = true;
                
                var propertyCount = reader.ReadInt();

                for (var i = 0; i < propertyCount; i++)
                {
                    var propertyName = reader.ReadString();
                    var propertyValue = reader.ReadBytes();

                    if (Properties.TryGetValue(propertyName, out var property))
                    {
                        property.RawData = propertyValue;
                    }
                    else
                    {
                        property = new();

                        property.Name = propertyName;
                        property.RawData = propertyValue;

                        property.FilePath = Path.Combine(ProfileDirectory, $"{propertyName}.profileProperty");

                        Properties.TryAdd(propertyName, property);
                    }
                }
            }
        });
        
        CommonLog.Debug("Player Profile Manager", $"[{UserId}] Finished handling update: SendToOthers={sendToOthers}; SetDirty={setDirty}");

        if (sendToOthers)
            message.SendToWhere(x => x.Connection.Id != scpInstance.Connection.Id);
        
        if (setDirty)
            PlayerProfileManager.SetDirty(this);
    }
}