using System.Net;

using CentralAPI.ClientPlugin.PlayerProfiles.Internal;
using CentralAPI.SharedLib;
using CentralAPI.SharedLib.PlayerProfiles;
using LabExtended.API;
using LabExtended.Core;
using LabExtended.Core.Pooling.Pools;
using NetworkLib;
using NetworkLib.Pools;
using NorthwoodLib.Pools;

namespace CentralAPI.ClientPlugin.PlayerProfiles;

/// <summary>
/// Represents a loaded player profile.
/// </summary>
public class PlayerProfileInstance
{
    /// <summary>
    /// Gets the associated player.
    /// </summary>
    public ExPlayer? Player { get; internal set; }

    /// <summary>
    /// Gets the user ID of this profile.
    /// </summary>
    public string UserId { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the timestamp of the profile's creation.
    /// </summary>
    public DateTime CreationTimestamp { get; internal set; } = DateTime.MinValue;

    /// <summary>
    /// Gets the timestamp of the profile's last activity.
    /// </summary>
    public DateTime ActivityTimestamp
    {
        get => field;
        internal set
        {
            if (field.Ticks == value.Ticks)
                return;

            field = value;

            DirtyFlags |= PlayerProfileUpdateType.Activity;
        }
    }
    
    /// <summary>
    /// Gets the username cache.
    /// </summary>
    public PlayerProfileCache<string> UsernameCache { get; private set; }
    
    /// <summary>
    /// Gets the address cache.
    /// </summary>
    public PlayerProfileCache<IPAddress> AddressCache { get; private set; }

    /// <summary>
    /// Gets all properties defined in this profile.
    /// </summary>
    public Dictionary<string, PlayerProfilePropertyBase> Properties { get; private set; } =
        DictionaryPool<string, PlayerProfilePropertyBase>.Shared.Rent();

    /// <summary>
    /// Gets a list of dirty properties.
    /// </summary>
    public List<PlayerProfilePropertyBase> DirtyProperties { get; private set; } =
        ListPool<PlayerProfilePropertyBase>.Shared.Rent();

    internal PlayerProfileInstance()
    {
        UsernameCache = new(this, PlayerProfileUpdateType.Username);
        AddressCache = new(this, PlayerProfileUpdateType.Address);
    }

    /// <summary>
    /// Returns the last username from <see cref="UsernameCache"/> (or an empty string if the cache is empty).
    /// </summary>
    public string LastName => UsernameCache.LastValue ?? string.Empty;

    /// <summary>
    /// Returns the last IP address from <see cref="AddressCache"/> (or null if the cache is empty).
    /// </summary>
    public IPAddress? LastAddress => AddressCache.LastValue;

    /// <summary>
    /// Whether or not any of the profile data is dirty.
    /// </summary>
    public bool IsDirty
    {
        get => field;
        internal set
        {
            if (field == value)
                return;

            field = value;

            if (value)
            {
                PlayerProfileManager.SetDirty(this);
            }
            else
            {
                DirtyProperties.Clear();
            }
        }
    }


    /// <summary>
    /// Gets the profile's dirty flags.
    /// </summary>
    public PlayerProfileUpdateType DirtyFlags
    {
        get => field;
        internal set
        {
            if (field == value)
                return;

            field = value;

            if (value == PlayerProfileUpdateType.None)
            {
                IsDirty = false;
                return;
            }

            IsDirty = true;
        }
    }

    /// <summary>
    /// Modifies the value of a property.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="modifier">The delegate used to modify the value.</param>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <returns>true if the value was modified</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool ModifyPropertyValue<T>(string name, Func<T, T> modifier)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        if (Properties.TryGetValue(name, out var property))
        {
            property.ObjectValue = modifier((T)property.ObjectValue);
            property.IsDirty = true;

            return true;
        }

        return false;
    }
    
    /// <summary>
    /// Sets the value of an existing property or adds a new one.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="value">The value of the property.</param>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <exception cref="ArgumentNullException"></exception>
    public void SetOrAddPropertyValue<T>(string name, object value) where T : PlayerProfilePropertyBase
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        
        if (Properties.TryGetValue(name, out var property))
        {
            property.ObjectValue = value;
            property.IsDirty = true;

            return;
        }
        
        if (!PlayerProfileManager.Properties.TryGetValue(name, out var propertyInfo))
            PlayerProfileManager.Properties.Add(name, propertyInfo = new(name, typeof(T)));

        property = propertyInfo.Create();

        property.Profile = this;
        property.ObjectValue = value;
        
        property.Initialize();
        
        if (Player?.ReferenceHub != null)
            property.OnJoined(Player);
        
        Properties.Add(name, property);

        property.IsDirty = true;
    }

    /// <summary>
    /// Sets the value of a property.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="value">The value to set.</param>
    /// <returns>true if the value was set</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool SetPropertyValue(string name, object value)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        if (Properties.TryGetValue(name, out var property))
        {
            property.ObjectValue = value;
            property.IsDirty = true;

            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the value of a specific property.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="KeyNotFoundException"></exception>
    public T GetPropertyValue<T>(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        
        if (Properties.TryGetValue(name, out var property))
            return (T)property.ObjectValue;
        
        throw new KeyNotFoundException($"Property {name} was not found in profile {UserId}");
    }

    /// <summary>
    /// Gets the value of a property or adds a new property with a default value.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="defaultValue">The default value of the property.</param>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <returns>The resolved property value or defaultValue</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public TValue GetOrAddPropertyValue<TValue, TProperty>(string name, TValue? defaultValue = default) where TProperty : PlayerProfilePropertyBase
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        
        if (Properties.TryGetValue(name, out var property))
            return (TValue)property.ObjectValue;
        
        if (!PlayerProfileManager.Properties.TryGetValue(name, out var propertyInfo))
            PlayerProfileManager.Properties.Add(name, propertyInfo = new(name, typeof(TProperty)));

        property = propertyInfo.Create();

        property.Profile = this;
        property.ObjectValue = defaultValue;
        
        property.Initialize();
        
        if (Player?.ReferenceHub != null)
            property.OnJoined(Player);
        
        Properties.Add(name, property);

        property.IsDirty = true;
        return defaultValue;
    }

    /// <summary>
    /// Gets a specific property.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <returns>The found property instance.</returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public T GetProperty<T>(string name) where T : PlayerProfilePropertyBase
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        
        if (Properties.TryGetValue(name, out var property))
            return (T)property;

        throw new KeyNotFoundException($"Property {name} was not found in profile {UserId}");
    }

    /// <summary>
    /// Gets or adds a specific property.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="defaultValue">The default value to set to the property.</param>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <returns>The found or created property instance.</returns>
    public T GetOrAddProperty<T>(string name, T? defaultValue = default) where T : PlayerProfilePropertyBase
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        
        if (Properties.TryGetValue(name, out var property))
            return (T)property;
        
        if (!PlayerProfileManager.Properties.TryGetValue(name, out var propertyInfo))
            PlayerProfileManager.Properties.Add(name, propertyInfo = new(name, typeof(T)));

        property = propertyInfo.Create();

        property.Profile = this;
        property.ObjectValue = defaultValue;
        
        property.Initialize();
        
        if (Player?.ReferenceHub != null)
            property.OnJoined(Player);
        
        Properties.Add(name, property);

        property.IsDirty = true;
        return (T)property;
    }

    internal void Destroy()
    {
        if (Properties != null)
        {
            foreach (var property in Properties)
            {
                property.Value.Destroy();
            }
            
            DictionaryPool<string, PlayerProfilePropertyBase>.Shared.Return(Properties);
        }
        
        if (DirtyProperties != null)
            ListPool<PlayerProfilePropertyBase>.Shared.Return(DirtyProperties);

        UsernameCache?.Destroy();
        AddressCache?.Destroy();

        UsernameCache = null;
        AddressCache = null;

        Properties = null;
        DirtyProperties = null;
    }

    internal void HandleUpdate(PlayerProfileUpdateMessage updateMessage)
    {
        var callModified = false;
        var clearDirty = false;
        
        updateMessage.Data?.ReadAction(reader =>
        {
            if ((updateMessage.Type & PlayerProfileUpdateType.Activity) == PlayerProfileUpdateType.Activity)
            {
                ActivityTimestamp = reader.ReadDate();

                clearDirty = true;
                callModified = true;
            }

            if ((updateMessage.Type & PlayerProfileUpdateType.Address) == PlayerProfileUpdateType.Address)
            {
                if (AddressCache.LastValue != null && AddressCache.LastTimestamp.Ticks != 0)
                    AddressCache.History[AddressCache.LastTimestamp] = AddressCache.LastValue;

                AddressCache.LastTimestamp = reader.ReadDate();
                AddressCache.LastValue = reader.ReadIpAddress();

                clearDirty = true;
                callModified = true;
            }

            if ((updateMessage.Type & PlayerProfileUpdateType.Username) == PlayerProfileUpdateType.Username)
            {
                if (!string.IsNullOrWhiteSpace(UsernameCache.LastValue) && UsernameCache.LastTimestamp.Ticks != 0)
                    UsernameCache.History[UsernameCache.LastTimestamp] = UsernameCache.LastValue;

                UsernameCache.LastTimestamp = reader.ReadDate();
                UsernameCache.LastValue = reader.ReadString();

                clearDirty = true;
                callModified = true;
            }

            if ((updateMessage.Type & PlayerProfileUpdateType.Property) == PlayerProfileUpdateType.Property)
            {
                var propertyCount = reader.ReadInt();

                for (var i = 0; i < propertyCount; i++)
                {
                    var propertyName = reader.ReadString();
                    var propertyData = reader.ReadBytes();

                    if (Properties.TryGetValue(propertyName, out var property))
                    {
                        DirtyProperties.Remove(property);
                        
                        propertyData.ReadAction(propertyReader => property.Read(propertyReader, false));
                        
                        PlayerProfileManager.OnPropertyModified(property);
                    }
                    else
                    {
                        if (!PlayerProfileManager.Properties.TryGetValue(propertyName, out var propertyInfo))
                        {
                            ApiLog.Warn("Player Profile Manager", $"Property &3{propertyName}&r is not registered!");
                            continue;
                        }

                        property = propertyInfo.Create();

                        property.Profile = this;
                        property.Initialize();
                        
                        Properties.Add(propertyName, property);
                        
                        if (Player?.ReferenceHub != null)
                            property.OnJoined(Player);
                        
                        propertyData.ReadAction(propertyReader => property.Read(propertyReader, true));
                        
                        PlayerProfileManager.OnPropertyModified(property);
                    }
                }
                
                callModified = true;
            }
        });

        if (clearDirty)
            IsDirty = false;

        if (callModified)
            PlayerProfileManager.OnProfileModified(this);
    }

    /// <summary>
    /// Creates a new profile instance for a specific player.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <returns>The created profile instance.</returns>
    public static PlayerProfileInstance CreateNew(ExPlayer player)
    {
        var profile = new PlayerProfileInstance();

        profile.UserId = player.UserId;
        profile.Player = player;
        
        profile.UsernameCache.SetNew(player.Nickname);
        profile.AddressCache.SetNew(IPAddress.Parse(player.IpAddress));

        foreach (var registeredProperty in PlayerProfileManager.Properties)
        {
            var property = registeredProperty.Value.Create();

            property.Profile = profile;
            property.Initialize();
            
            property.OnJoined(player);
            
            profile.Properties.Add(registeredProperty.Key, property);
        }
        
        profile.DirtyFlags = PlayerProfileUpdateType.None;
        profile.DirtyProperties.Clear();

        return profile;
    }
}