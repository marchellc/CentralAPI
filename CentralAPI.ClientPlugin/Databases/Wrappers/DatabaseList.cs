using NetworkLib;

namespace CentralAPI.ClientPlugin.Databases.Wrappers;

/// <summary>
/// Reads a list.
/// </summary>
/// <typeparam name="T"></typeparam>
public class DatabaseList<T> : DatabaseWrapper<List<T>>
{
    /// <summary>
    /// Gets the wrapper used to read items.
    /// </summary>
    public DatabaseWrapper<T> ItemWrapper { get; }
    
    /// <summary>
    /// Creates a new <see cref="DatabaseList{T}"/> instance.
    /// </summary>
    /// <param name="itemWrapper">The underlying item wrapper.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public DatabaseList(DatabaseWrapper<T> itemWrapper)
    {
        if (itemWrapper is null)
            throw new ArgumentNullException(nameof(itemWrapper));
        
        ItemWrapper = itemWrapper;
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Read"/>
    public override void Read(NetworkReader reader, ref List<T> value)
    {
        if (value is null)
            value = new();
        else
            value.Clear();

        var count = reader.ReadInt();

        if (value.Capacity < count)
            value.Capacity = count;

        for (var i = 0; i < count; i++)
        {
            var tempValue = default(T);
            
            ItemWrapper.Read(reader, ref tempValue);
            
            if (tempValue != null)
                value.Add(tempValue);
        }
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Write"/>
    public override void Write(NetworkWriter writer, ref List<T> value)
    {
        if (value != null)
        {
            writer.WriteInt(value.Count);

            for (var i = 0; i < value.Count; i++)
            {
                var tempValue = value[i];
                
                ItemWrapper.Write(writer, ref tempValue);
            }
        }
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Compare"/>
    public override bool Compare(ref List<T> value, ref List<T> other)
    {
        if (value is null || other is null)
            return false;
        
        if (value.Count != other.Count)
            return false;

        for (var i = 0; i < value.Count; i++)
        {
            var tempValue = value[i];
            var tempOther = other[i];
            
            if (!ItemWrapper.Compare(ref tempValue, ref tempOther))
            {
                return false;
            }
        }

        return true;
    }
}