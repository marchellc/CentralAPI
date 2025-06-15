namespace CentralAPI.ClientPlugin.Databases.Extensions;

/// <summary>
/// Extensions for database collections targeting supported enumerables.
/// </summary>
public static class EnumerableCollectionExtensions
{
    public static List<T> AccessList<T>(this DatabaseCollection<List<T>> collection, string key, Func<List<T>> defaultFactory, Action<List<T>> accessor)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));
        
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));
        
        if (defaultFactory is null)
            throw new ArgumentNullException(nameof(defaultFactory));

        if (accessor is null)
            throw new ArgumentNullException(nameof(accessor));

        return collection.UpdateOrAdd(key, _ => false, (ref List<T> list, bool isNew) =>
        {
            if (isNew)
                list = defaultFactory();

            accessor(list);
        });
    }
}