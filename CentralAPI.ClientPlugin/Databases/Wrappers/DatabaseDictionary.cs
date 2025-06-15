using NetworkLib;

namespace CentralAPI.ClientPlugin.Databases.Wrappers;

/// <summary>
/// Reads a dictionary.
/// </summary>
public class DatabaseDictionary<TKey, TValue> : DatabaseWrapper<Dictionary<TKey, TValue>>
{
    /// <summary>
    /// Gets the wrapper used to read keys.
    /// </summary>
    public DatabaseWrapper<TKey> KeyWrapper { get; }
    
    /// <summary>
    /// Gets the wrapper used to read values.
    /// </summary>
    public DatabaseWrapper<TValue> ValueWrapper { get; }
    
    /// <summary>
    /// Creates a new <see cref="DatabaseList{T}"/> instance.
    /// </summary>
    /// <param name="keyWrapper">The underlying key wrapper.</param>
    /// <param name="valueWrapper">The underlying value wrapper.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public DatabaseDictionary(DatabaseWrapper<TKey> keyWrapper, DatabaseWrapper<TValue> valueWrapper)
    {
        if (keyWrapper is null)
            throw new ArgumentNullException(nameof(keyWrapper));
        
        if (valueWrapper is null)
            throw new ArgumentNullException(nameof(valueWrapper));
        
        KeyWrapper = keyWrapper;
        ValueWrapper = valueWrapper;
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Read"/>
    public override void Read(NetworkReader reader, ref Dictionary<TKey, TValue> value)
    {
        if (value is null)
            value = new();
        else
            value.Clear();

        var count = reader.ReadInt();

        for (var i = 0; i < count; i++)
        {
            var tempKey = default(TKey);
            var tempValue = default(TValue);
            
            KeyWrapper.Read(reader, ref tempKey);
            ValueWrapper.Read(reader, ref tempValue);
            
            if (tempKey != null)
                value.Add(tempKey, tempValue);
        }
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Write"/>
    public override void Write(NetworkWriter writer, ref Dictionary<TKey, TValue> value)
    {
        if (value != null)
        {
            writer.WriteInt(value.Count);

            foreach (var pair in value)
            {
                var tempKey = pair.Key;
                var tempValue = pair.Value;
                
                KeyWrapper.Write(writer, ref tempKey);
                ValueWrapper.Write(writer, ref tempValue);
            }
        }
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Compare"/>
    public override bool Compare(ref Dictionary<TKey, TValue> value, ref Dictionary<TKey, TValue> other)
    {
        if (value is null || other is null)
            return false;
        
        if (value.Count != other.Count)
            return false;

        foreach (var pair in value)
        {
            if (!other.TryGetValue(pair.Key, out var otherValue))
                return false;

            var tempValue = pair.Value;

            if (!ValueWrapper.Compare(ref tempValue, ref otherValue))
                return false;
        }

        return true;
    }
}