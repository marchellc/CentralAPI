using System.Collections.Concurrent;
using CentralAPI.SharedLib;
using NetworkLib;

namespace CentralAPI.ServerApp.PlayerProfiles;

/// <summary>
/// Represents a cached value in a profile.
/// </summary>
public class PlayerProfileCache<T>
{
    /// <summary>
    /// Gets the last assigned value.
    /// </summary>
    public T LastValue;
    
    /// <summary>
    /// Gets the timestamp of the last value.
    /// </summary>
    public DateTime LastTimestamp;

    /// <summary>
    /// Gets the value history.
    /// </summary>
    public volatile ConcurrentDictionary<DateTime, T> History = new();

    /// <summary>
    /// Sets a new value.
    /// </summary>
    /// <param name="newValue">The value to send.</param>
    /// <param name="cacheCurrent">Whether or not to cache the current value.</param>
    public void SetNew(T newValue, bool cacheCurrent = true)
    {
        if (cacheCurrent && LastValue != null && LastTimestamp != DateTime.MinValue && !History.ContainsKey(LastTimestamp))
            History.TryAdd(LastTimestamp, LastValue);
        
        LastValue = newValue;
        LastTimestamp = DateTime.UtcNow;

        if (History.IsEmpty)
            History.TryAdd(LastTimestamp, LastValue);
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
        
        var historySize = reader.ReadInt();
        var hasValue = reader.ReadBool();

        if (hasValue)
            LastTimestamp = reader.ReadDate();

        for (var i = 0; i < historySize; i++)
        {
            var value = valueReader();
            var stamp = reader.ReadDate();
            
            History.TryAdd(stamp, value);

            if (hasValue && stamp.Ticks == LastTimestamp.Ticks)
                LastValue = value;
        }
    }

    /// <summary>
    /// Writes the cache.
    /// </summary>
    /// <param name="writer">The target writer.</param>
    /// <param name="valueWriter">The value writer delegate.</param>
    public void Write(NetworkWriter writer, Action<T> valueWriter)
    {
        if (writer is null)
            throw new ArgumentNullException(nameof(writer));
        
        if (valueWriter is null)
            throw new ArgumentNullException(nameof(valueWriter));
        
        writer.WriteInt(History.Count);

        if (LastTimestamp != DateTime.MinValue)
        {
            writer.WriteBool(true);
            writer.WriteDate(LastTimestamp);
        }
        else
        {
            writer.WriteBool(false);
        }

        foreach (var pair in History)
        {
            valueWriter(pair.Value);
            
            writer.WriteDate(pair.Key);
        }
    }
}