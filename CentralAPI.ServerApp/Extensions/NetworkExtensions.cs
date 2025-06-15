using NetworkLib;

namespace CentralAPI.ServerApp.Extensions;

/// <summary>
/// Extensions targeting classes from NetworkLib.
/// </summary>
public static class NetworkExtensions
{
    /// <summary>
    /// Whether or not one writer is equal to another.
    /// </summary>
    /// <param name="writer">The source writer.</param>
    /// <param name="other">The target writer.</param>
    /// <returns>true if the writers contain the same data</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool IsEqual(this NetworkWriter writer, NetworkWriter other)
    {
        if (writer is null)
            throw new ArgumentNullException(nameof(writer));
        
        if (other is null)
            throw new ArgumentNullException(nameof(other));

        if (writer.Count != other.Count)
            return false;

        for (var i = 0; i < writer.Count; i++)
        {
            if (writer.Buffer[i] != other.Buffer[i])
            {
                return false;
            }
        }

        return true;
    }
}