using NetworkLib;

namespace CentralAPI.ClientPlugin.Databases;

/// <summary>
/// Wraps around objects to provide writer and reader methods, as well as comparison.
/// </summary>
/// <typeparam name="T">The object's type.</typeparam>
public abstract class DatabaseWrapper<T>
{
    /// <summary>
    /// Reads the value.
    /// </summary>
    /// <param name="reader">The target reader.</param>
    /// <param name="value">The value to read to.</param>
    public abstract void Read(NetworkReader reader, ref T value);
    
    /// <summary>
    /// Writes the value.
    /// </summary>
    /// <param name="writer">The target value.</param>
    /// <param name="value">The value to write.</param>
    public abstract void Write(NetworkWriter writer, ref T value);

    /// <summary>
    /// Compares two values.
    /// </summary>
    /// <param name="value">The first value.</param>
    /// <param name="other">The second value.</param>
    /// <returns>true if the values are the same.</returns>
    public abstract bool Compare(ref T value, ref T other);

    /// <summary>
    /// Converts the specified value to a string.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="result">The converted value.</param>
    public virtual void Convert(T value, out string result)
    {
        result = $"ConversionNotImplemented ({GetType().Name})";
    }
}