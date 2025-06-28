using CentralAPI.SharedLib;
using CentralAPI.SharedLib.PlayerProfiles;

using LabExtended.Core.Pooling.Pools;

using NetworkLib;

namespace CentralAPI.ClientPlugin.PlayerProfiles;

/// <summary>
/// Represents a value with a cached history.
/// </summary>
/// <typeparam name="T">The value type.</typeparam>
public class PlayerProfileCache<T>
{
    internal PlayerProfileCache(PlayerProfileInstance profile, PlayerProfileUpdateType dirtyFlag)
    {
        if (profile is null)
            throw new ArgumentNullException(nameof(profile));

        Profile = profile;
        DirtyFlag = dirtyFlag;
    }

    /// <summary>
    /// Gets the cache history.
    /// </summary>
    public Dictionary<DateTime, T> History { get; private set; } = DictionaryPool<DateTime, T>.Shared.Rent();
    
    /// <summary>
    /// Gets the parent profile.
    /// </summary>
    public PlayerProfileInstance Profile { get; }
    
    /// <summary>
    /// Gets the dirty flag of the cache.
    /// </summary>
    public PlayerProfileUpdateType DirtyFlag { get; }
    
    /// <summary>
    /// Gets the last value.
    /// </summary>
    public T LastValue { get; internal set; }

    /// <summary>
    /// Gets the timestamp of the last value.
    /// </summary>
    public DateTime LastTimestamp { get; internal set; } = DateTime.MinValue;

    /// <summary>
    /// Sets a new value.
    /// </summary>
    /// <param name="newValue">The new value to set.</param>
    /// <param name="cacheCurrent">Whether or not to cache the current value.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void SetNew(T newValue, bool cacheCurrent = true)
    {
        if (newValue is null)
            throw new ArgumentNullException(nameof(newValue));

        if (LastValue != null && LastTimestamp != DateTime.MinValue && cacheCurrent && !History.ContainsKey(LastTimestamp))
            History[LastTimestamp] = LastValue;
        
        LastValue = newValue;
        LastTimestamp = DateTime.UtcNow;
        
        if (History.Count == 0)
            History.Add(LastTimestamp, LastValue);

        Profile.DirtyFlags |= DirtyFlag;
    }
    
    /// <summary>
    /// Reads the cache.
    /// </summary>
    /// <param name="reader">The target reader.</param>
    /// <param name="valueReader">The value reader delegate-</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Read(NetworkReader reader, Func<T> valueReader)
    {
        if (reader is null)
            throw new ArgumentNullException(nameof(reader));
        
        if (valueReader is null)
            throw new ArgumentNullException(nameof(valueReader));
        
        History.Clear();
        
        var historySize = reader.ReadInt();
        var hasValue = reader.ReadBool();

        if (hasValue)
            LastTimestamp = reader.ReadDate();

        for (var i = 0; i < historySize; i++)
        {
            var value = valueReader();
            var stamp = reader.ReadDate();
            
            History.Add(stamp, value);

            if (hasValue && stamp.Ticks == LastTimestamp.Ticks)
                LastValue = value;
        }
    }

    /// <summary>
    /// Destroys the cache.
    /// </summary>
    public void Destroy()
    {
        if (History != null)
            DictionaryPool<DateTime, T>.Shared.Return(History);

        History = null;

        LastValue = default;
        LastTimestamp = DateTime.MinValue;
    }
}