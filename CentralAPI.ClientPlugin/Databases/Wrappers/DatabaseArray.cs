using LabExtended.Extensions;
using NetworkLib;
using NorthwoodLib.Pools;

namespace CentralAPI.ClientPlugin.Databases.Wrappers;

/// <summary>
/// Reads an array.
/// </summary>
/// <typeparam name="T"></typeparam>
public class DatabaseArray<T> : DatabaseWrapper<T[]>
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
    public DatabaseArray(DatabaseWrapper<T> itemWrapper)
    {
        if (itemWrapper is null)
            throw new ArgumentNullException(nameof(itemWrapper));
        
        ItemWrapper = itemWrapper;
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Read"/>
    public override void Read(NetworkReader reader, ref T[] value)
    {
        var count = reader.ReadInt();
        
        value = new T[count];

        for (var i = 0; i < count; i++)
            ItemWrapper.Read(reader, ref value[i]);
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Write"/>
    public override void Write(NetworkWriter writer, ref T[] value)
    {
        if (value != null)
        {
            writer.WriteInt(value.Length);

            for (var i = 0; i < value.Length; i++)
                ItemWrapper.Write(writer, ref value[i]);
        }
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Compare"/>
    public override bool Compare(ref T[] value, ref T[] other)
    {
        if (value is null || other is null)
            return false;
        
        if (value.Length != other.Length)
            return false;

        for (var i = 0; i < value.Length; i++)
        {
            if (!ItemWrapper.Compare(ref value[i], ref other[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc cref="DatabaseWrapper{T}.Convert"/>
    public override void Convert(T[] value, out string result)
    {
        if (value is null)
        {
            result = "(null)";
            return;
        }

        result = $"Array ({value.Length} item(s))";
    }
}