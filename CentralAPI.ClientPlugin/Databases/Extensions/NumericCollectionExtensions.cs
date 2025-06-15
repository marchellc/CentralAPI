namespace CentralAPI.ClientPlugin.Databases.Extensions;

/// <summary>
/// Extensions targeting database collections with numerical types.
/// </summary>
public static class NumericCollectionExtensions
{
    public static byte IncrementByte(this DatabaseCollection<byte> collection, string key, byte defaultValue, byte incrementBy)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));
        
        if (incrementBy <= 0)
            throw new ArgumentOutOfRangeException(nameof(incrementBy));
        
        return collection.UpdateOrAdd(key, _ => true, (ref byte value, bool isNew) =>
        {
            if (isNew)
            {
                value = (byte)(defaultValue + incrementBy);
                return;
            }

            value += incrementBy;
        });
    }
    
    public static sbyte IncrementSByte(this DatabaseCollection<sbyte> collection, string key, sbyte defaultValue, sbyte incrementBy)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));
        
        return collection.UpdateOrAdd(key, _ => true, (ref sbyte value, bool isNew) =>
        {
            if (isNew)
            {
                value = (sbyte)(defaultValue + incrementBy);
                return;
            }

            value += incrementBy;
        });
    }
    
    public static short IncrementInt16(this DatabaseCollection<short> collection, string key, short defaultValue, short incrementBy)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));
        
        return collection.UpdateOrAdd(key, _ => true, (ref short value, bool isNew) =>
        {
            if (isNew)
            {
                value = (short)(defaultValue + incrementBy);
                return;
            }

            value += incrementBy;
        });
    }
    
    public static ushort IncrementUInt16(this DatabaseCollection<ushort> collection, string key, ushort defaultValue, ushort incrementBy)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));
        
        if (incrementBy <= 0)
            throw new ArgumentOutOfRangeException(nameof(incrementBy));
        
        return collection.UpdateOrAdd(key, _ => true, (ref ushort value, bool isNew) =>
        {
            if (isNew)
            {
                value = (byte)(defaultValue + incrementBy);
                return;
            }

            value += incrementBy;
        });
    }
    
    public static int IncrementInt32(this DatabaseCollection<int> collection, string key, int defaultValue, int incrementBy)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));
        
        return collection.UpdateOrAdd(key, _ => true, (ref int value, bool isNew) =>
        {
            if (isNew)
            {
                value = defaultValue + incrementBy;
                return;
            }

            value += incrementBy;
        });
    }
    
    public static uint IncrementUInt32(this DatabaseCollection<uint> collection, string key, uint defaultValue, uint incrementBy)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));
        
        if (incrementBy <= 0)
            throw new ArgumentOutOfRangeException(nameof(incrementBy));
        
        return collection.UpdateOrAdd(key, _ => true, (ref uint value, bool isNew) =>
        {
            if (isNew)
            {
                value = defaultValue + incrementBy;
                return;
            }

            value += incrementBy;
        });
    }
    
    public static long IncrementInt64(this DatabaseCollection<long> collection, string key, long defaultValue, long incrementBy)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));
        
        return collection.UpdateOrAdd(key, _ => true, (ref long value, bool isNew) =>
        {
            if (isNew)
            {
                value = defaultValue + incrementBy;
                return;
            }

            value += incrementBy;
        });
    }
    
    public static ulong IncrementUInt64(this DatabaseCollection<ulong> collection, string key, ulong defaultValue, ulong incrementBy)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));
        
        if (incrementBy <= 0)
            throw new ArgumentOutOfRangeException(nameof(incrementBy));
        
        return collection.UpdateOrAdd(key, _ => true, (ref ulong value, bool isNew) =>
        {
            if (isNew)
            {
                value = defaultValue + incrementBy;
                return;
            }

            value += incrementBy;
        });
    }
    
    public static float IncrementFloat(this DatabaseCollection<float> collection, string key, float defaultValue, float incrementBy)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));
        
        return collection.UpdateOrAdd(key, _ => true, (ref float value, bool isNew) =>
        {
            if (isNew)
            {
                value = defaultValue + incrementBy;
                return;
            }

            value += incrementBy;
        });
    }
    
    public static double IncrementDouble(this DatabaseCollection<double> collection, string key, double defaultValue, double incrementBy)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));
        
        return collection.UpdateOrAdd(key, _ => true, (ref double value, bool isNew) =>
        {
            if (isNew)
            {
                value = defaultValue + incrementBy;
                return;
            }

            value += incrementBy;
        });
    }
}