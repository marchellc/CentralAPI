using System.Collections.Concurrent;

namespace CentralAPI.ServerApp.Extensions;

/// <summary>
/// Extensions for collections.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Removes an item from a bag.
    /// </summary>
    /// <param name="bag">The target bag.</param>
    /// <param name="item">The item to remove.</param>
    /// <typeparam name="T">The item type.</typeparam>
    public static void Remove<T>(this ConcurrentBag<T> bag, T item)
    {
        var list = new List<T>();

        foreach (var current in bag)
        {
            if (!current.Equals(item))
            {
                list.Add(item);
            }
        }
        
        bag.Clear();
        
        list.ForEach(current => bag.Add(current));
    }
}