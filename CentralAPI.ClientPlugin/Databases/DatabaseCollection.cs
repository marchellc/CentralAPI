using CentralAPI.ClientPlugin.Databases.Internal;
using CentralAPI.ClientPlugin.Databases.Requests;
using LabExtended.Core.Pooling.Pools;
using LabExtended.Extensions;

using NetworkLib.Pools;

using NorthwoodLib.Pools;

namespace CentralAPI.ClientPlugin.Databases;

/// <summary>
/// Used to update collection items.
/// </summary> 
public delegate void UpdateDelegate<T>(ref T item, bool isNew);

/// <summary>
/// Used to iterate over collection items.
/// </summary> 
public delegate void IterateDelegate<T>(string name, ref T item, out bool shouldStop, out bool isUpdated);

/// <summary>
/// Represents a collection in a database table which stores items.
/// </summary>
public class DatabaseCollection<T> : DatabaseCollectionBase
{
    /// <summary>
    /// Gets called once a new item is added to a collection.
    /// </summary>
    public static event Action<DatabaseCollection<T>, T>? Added; 
    
    /// <summary>
    /// Gets called once an item is removed from a collection.
    /// </summary>
    public static event Action<DatabaseCollection<T>, T>? Removed;

    /// <summary>
    /// Gets called once an item is updated.
    /// </summary>
    public static event Action<DatabaseCollection<T>, T?>? Updated; 
    
    private Dictionary<string, DatabaseItem<T>> items = new();
    private DatabaseWrapper<T> wrapper;

    internal override Type ItemType { get; } = typeof(DatabaseItem<>).MakeGenericType(typeof(T));

    /// <inheritdoc cref="DatabaseCollectionBase.Size"/>
    public override int Size => items.Count;
    
    /// <summary>
    /// Whether or not the collection is empty.
    /// </summary>
    public bool IsEmpty => items.Count == 0;

    /// <summary>
    /// Gets the internal wrapper.
    /// </summary>
    public DatabaseWrapper<T> Wrapper => wrapper;

    /// <summary>
    /// Gets a reference to a specific item.
    /// </summary>
    /// <param name="itemName">The name of the item.</param>
    /// <returns>The item's reference.</returns>
    /// <exception cref="ArgumentNullException">Item name is null or empty</exception>
    /// <exception cref="Exception">Item was not found in collection</exception>
    public DatabaseItem<T> GetReference(string itemName)
    {
        if (string.IsNullOrEmpty(itemName))
            throw new ArgumentNullException(nameof(itemName));

        if (!items.TryGetValue(itemName, out var item))
            throw new Exception($"Item '{itemName}' was not found");

        return item;
    }

    /// <summary>
    /// Gets a reference to a specific item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The reference to the item.</returns>
    /// <exception cref="ArgumentNullException">item is null</exception>
    /// <exception cref="Exception">No matching items were found.</exception>
    public DatabaseItem<T> GetReference(T item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        foreach (var pair in items)
        {
            if (wrapper.Compare(ref pair.Value.value, ref item))
            {
                return pair.Value;
            }
        }

        throw new Exception("Item not found");
    }
    
    /// <summary>
    /// Iterates over each item in a collection matching a condition.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <param name="iterateDelegate">The delegate used to iterate over items.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void ForEachWhere(Predicate<T> predicate, IterateDelegate<T> iterateDelegate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));
        
        if (iterateDelegate is null)
            throw new ArgumentNullException(nameof(iterateDelegate));

        foreach (var item in items)
        {
            if (!predicate(item.Value.value))
                continue;
            
            iterateDelegate(item.Key, ref item.Value.value, out var shouldStop, out var isUpdated);

            if (isUpdated)
            {
                Updated?.InvokeSafe(this, item.Value.value);   
                
                AddItemRequest.SendRequest(Table.Id, Id, item.Key, true, writer => wrapper.Write(writer, ref item.Value.value));
            }

            if (shouldStop)
                return;
        }
    }

    /// <summary>
    /// Iterates over each item in a collection.
    /// </summary>
    /// <param name="iterateDelegate">The delegate used to iterate over items.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void ForEach(IterateDelegate<T> iterateDelegate)
    {
        if (iterateDelegate is null)
            throw new ArgumentNullException(nameof(iterateDelegate));

        foreach (var item in items)
        {
            iterateDelegate(item.Key, ref item.Value.value, out var shouldStop, out var isUpdated);

            if (isUpdated)
            {
                Updated?.InvokeSafe(this, item.Value.value);
                
                AddItemRequest.SendRequest(Table.Id, Id, item.Key, true, writer => wrapper.Write(writer, ref item.Value.value));
            }

            if (shouldStop)
                return;
        }
    }

    /// <summary>
    /// Returns matching items in a list.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <returns>The list of matching items.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public List<T> Where(Predicate<T> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));
        
        var list = new List<T>();
        
        WhereNonAlloc(predicate, list);
        return list;
    }

    /// <summary>
    /// Returns matching items in a <b>POOLED</b> list.
    /// <remarks><b>Return the list to it's pooled once finished (<see cref="ListPool{T}.Shared"/>)!</b></remarks>
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <returns>The list of matching items.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public List<T> WhereNonAlloc(Predicate<T> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        var list = ListPool<T>.Shared.Rent();
        
        WhereNonAlloc(predicate, list);
        return list;
    }

    /// <summary>
    /// Adds matching items into a list.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <param name="target">The target list.</param>
    /// <exception cref="ArgumentNullException">predicate or target is null</exception>
    public void WhereNonAlloc(Predicate<T> predicate, IList<T> target)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        if (target is null)
            throw new ArgumentNullException(nameof(target));

        foreach (var item in items)
        {
            if (predicate(item.Value.value))
            {
                target.Add(item.Value.value);
            }
        }
    }

    /// <summary>
    /// Removes all items matching a predicate.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <returns>Number of removed items.</returns>
    /// <exception cref="ArgumentNullException">predicate is null</exception>
    public int RemoveWhere(Predicate<T> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        var itemsToRemove = ListPool<string>.Shared.Rent();

        foreach (var item in items)
        {
            if (predicate(item.Value.value))
            {
                itemsToRemove.Add(item.Key);
                
                Removed?.InvokeSafe(this, item.Value.value);
                
                item.Value.Collection = null;
                item.Value.collection = null;
            }
        }
        
        itemsToRemove.ForEach(x => items.Remove(x));
        
        RemoveItemRequest.SendRequest(Table.Id, Id, writer =>
        {
            writer.WriteByte((byte)itemsToRemove.Count);
            
            foreach (var itemName in itemsToRemove)
                writer.WriteString(itemName);
        });

        var count = itemsToRemove.Count;
        
        ListPool<string>.Shared.Return(itemsToRemove);
        return count;
    }

    /// <summary>
    /// Removes an item.
    /// </summary>
    /// <param name="name">The name of the item to remove.</param>
    /// <returns>true if the item was removed</returns>
    public bool Remove(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        
        if (items.TryGetValue(name, out var item)
            && items.Remove(name))
        {
            item.Collection = null;
            item.collection = null;
            
            Removed?.InvokeSafe(this, item.value);
            
            RemoveItemRequest.SendRequest(Table.Id, Id, writer =>
            {
                writer.WriteByte(1);
                writer.WriteString(item.Name);
            });
            
            return true;
        }

        return false;
    }

    /// <summary>
    /// Updates an existing value or adds a new one.
    /// </summary>
    /// <param name="name">The name of the item.</param>
    /// <param name="comparer">The comparer, should return true if the item does not require an update.</param>
    /// <param name="updateDelegate">The delegate used to update the item.</param>
    /// <returns>the updated or created item</returns>
    public T UpdateOrAdd(string name, Predicate<T> comparer, UpdateDelegate<T> updateDelegate)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));
        
        if (comparer is null)
            throw new ArgumentNullException(nameof(comparer));

        if (updateDelegate is null)
            throw new ArgumentNullException(nameof(updateDelegate));

        if (items.TryGetValue(name, out var item))
        {
            if (comparer(item.value))
                return item.value;

            updateDelegate(ref item.value, false);

            Updated?.InvokeSafe(this, item.value);
            
            AddItemRequest.SendRequest(Table.Id, Id, item.Name, true, writer => wrapper.Write(writer, ref item.value));
            return item.value;
        }

        item = new();
        item.Name = name;

        item.collection = this;
        item.Collection = this;

        updateDelegate(ref item.value, true);

        item.hasRead = true;
        
        items.Add(name, item);
        
        Added?.InvokeSafe(this, item.value);
        
        AddItemRequest.SendRequest(Table.Id, Id, name, false, writer => wrapper.Write(writer, ref item.value));
        return item.value;
    }

    /// <summary>
    /// Updates an existing value.
    /// </summary>
    /// <param name="predicate">The predicate used to search for the value.</param>
    /// <param name="comparer">The comparer, should return true if the item does not require an update.</param>
    /// <param name="updateDelegate">The delegate used to update the item's value.</param>
    /// <returns>true if the item was updated</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool Update(Predicate<T> predicate, Predicate<T> comparer, UpdateDelegate<T> updateDelegate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        if (comparer is null)
            throw new ArgumentNullException(nameof(comparer));

        if (updateDelegate is null)
            throw new ArgumentNullException(nameof(updateDelegate));

        if (TryGetDb(predicate, out var item))
        {
            if (comparer(item.Value))
                return false;

            updateDelegate(ref item.value, false);
            
            Updated?.InvokeSafe(this, item.value);
            
            AddItemRequest.SendRequest(Table.Id, Id, item.Name, true, writer => wrapper.Write(writer, ref item.value));
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets or adds an item by it's name.
    /// </summary>
    /// <param name="name">The name of the item.</param>
    /// <param name="factory">The factory used to create a new item.</param>
    /// <returns>The found or added item.</returns>
    /// <exception cref="ArgumentNullException">name is null or empty or factory is null</exception>
    public T GetOrAdd(string name, Func<T> factory)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        if (factory is null)
            throw new ArgumentNullException(nameof(factory));

        if (items.TryGetValue(name, out var item))
            return item.value;

        item = new();
        item.Name = name;

        item.collection = this;
        item.value = factory();

        item.hasRead = true;

        item.Collection = this;
        
        items.Add(name, item);
        
        Added?.InvokeSafe(this, item.value);
        
        AddItemRequest.SendRequest(Table.Id, Id, name, false, writer => wrapper.Write(writer, ref item.value));
        return item.value;
    }

    /// <summary>
    /// Gets an item by a predicate.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <returns>The found item.</returns>
    /// <exception cref="ArgumentNullException">predicate is null</exception>
    /// <exception cref="Exception">No items match the predicate</exception>
    public T Get(Predicate<T> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        if (TryGet(predicate, out var item))
            return item;

        throw new Exception($"Could not find item matching the provided predicate: {predicate.Method?.GetMemberName() ?? "null"}.");
    }

    /// <summary>
    /// Gets an item by it's key.
    /// </summary>
    /// <param name="key">The item's key.</param>
    /// <returns>The found item.</returns>
    /// <exception cref="ArgumentNullException">key is null or empty</exception>
    /// <exception cref="KeyNotFoundException">key was not found</exception>
    public T Get(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        if (TryGet(key, out var item))
            return item;

        throw new KeyNotFoundException($"Key '{key}' not found");
    }

    /// <summary>
    /// Attempts to find an item by a predicate.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <param name="item">The found item.</param>
    /// <returns>true if the item was found</returns>
    /// <exception cref="ArgumentNullException">predicate is null</exception>
    public bool TryGet(Predicate<T> predicate, out T item)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));
        
        foreach (var pair in items)
        {
            if (predicate(pair.Value.Value))
            {
                item = pair.Value.Value;
                return true;
            }
        }

        item = default;
        return false;
    }

    /// <summary>
    /// Attempts to find an item by it's key.
    /// </summary>
    /// <param name="key">The item's key.</param>
    /// <param name="item">The found item.</param>
    /// <returns>true if the item was found</returns>
    /// <exception cref="ArgumentNullException">key is null or empty</exception>
    public bool TryGet(string key, out T item)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        if (items.TryGetValue(key, out var value))
        {
            item = value.Value;
            return true;
        }

        item = default;
        return false;
    }

    /// <summary>
    /// Clears the collection.
    /// </summary>
    public void Clear()
        => Table?.ClearCollection(Id);

    /// <summary>
    /// Drops the collection.
    /// </summary>
    public void Drop()
        => Table?.DropCollection(Id);

    internal void OnUpdated(DatabaseItem<T> item)
    {
        Updated?.InvokeSafe(this, item.value);
        
        AddItemRequest.SendRequest(Table.Id, Id, item.Name, true, writer => wrapper.Write(writer, ref item.value));
    }

    internal override bool InternalTryGet(string name, out DatabaseItemBase item)   
        => items.TryGetValue(name, out item);

    internal override void InternalInit()
    {
        if (!DatabaseDirector.TryGetWrapper(out wrapper))
            throw new Exception($"No wrapper registered for type '{typeof(T).FullName}'");
    }

    internal override void InternalAddItem(DatabaseItemBase item)
    {
        if (item is DatabaseItem<T> genericItem)
        {
            genericItem.collection = this;
            genericItem.Collection = this;

            if (!genericItem.hasRead && genericItem.reader != null)
            {
                wrapper.Read(genericItem.reader, ref genericItem.value);

                genericItem.hasRead = true;
                
                genericItem.reader?.Return();
                genericItem.reader = null;
            }
            
            items.Add(genericItem.Name, genericItem);
            
            Added?.InvokeSafe(this, genericItem.Value);
        }
    }

    internal override void InternalRemoveItem(DatabaseItemBase item)
    {
        if (items.TryGetValue(item.Name, out var genericItem))
        {
            Removed?.InvokeSafe(this, genericItem.Value);
            
            items.Remove(item.Name);
        }
    }

    internal override void InternalUpdate(DatabaseItemBase item)
    {
        if (item is DatabaseItem<T> genericItem)
        {
            Updated?.InvokeSafe(this, genericItem.value);
        }
    }

    internal override void InternalClear()
    {
        if (items != null)
        {
            foreach (var item in items)
            {
                Removed?.Invoke(this, item.Value.value);

                item.Value.collection = null;
                item.Value.Collection = null;
            }
            
            items.Clear();
        }
    }

    internal override void InternalDestroy(bool isDrop = false)
    {
        if (items != null)
        {
            if (isDrop)
            {
                foreach (var item in items)
                {
                    Removed?.Invoke(this, item.Value.value);

                    item.Value.collection = null;
                    item.Value.Collection = null;
                }
            }
            
            DictionaryPool<string, DatabaseItem<T>>.Shared.Return(items);
        }

        items = null;
    }

    private bool TryGetDb(Predicate<T> predicate, out DatabaseItem<T> item)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));
        
        foreach (var pair in items)
        {
            if (predicate(pair.Value.Value))
            {
                item = pair.Value;
                return true;
            }
        }

        item = default;
        return false;
    }
}