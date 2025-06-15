using CentralAPI.ClientPlugin.Databases.Internal;
using CentralAPI.ClientPlugin.Databases.Requests;
using LabExtended.Core.Pooling.Pools;
using LabExtended.Extensions;

namespace CentralAPI.ClientPlugin.Databases;

/// <summary>
/// Represents a table in a database which stores collections.
/// </summary>
public class DatabaseTable
{
    internal Dictionary<byte, DatabaseCollectionBase> collections = DictionaryPool<byte, DatabaseCollectionBase>.Shared.Rent();

    /// <summary>
    /// Gets called once a new collection is added.
    /// </summary>
    public static event Action<DatabaseTable, DatabaseCollectionBase>? Added;

    /// <summary>
    /// Gets called once a collection is dropped.
    /// </summary>
    public static event Action<DatabaseTable, DatabaseCollectionBase>? Dropped;

    /// <summary>
    /// Gets called once a collection is cleared.
    /// </summary>
    public static event Action<DatabaseTable, DatabaseCollectionBase>? Cleared; 
    
    /// <summary>
    /// Gets the table's ID.
    /// </summary>
    public byte Id { get; internal set; }

    /// <summary>
    /// Gets the number of stored collections.
    /// </summary>
    public int Size => collections?.Count ?? 0;
    
    /// <summary>
    /// Attempts to find a collection by an ID.
    /// </summary>
    /// <param name="collectionId">The ID of the collection.</param>
    /// <param name="collection">The found collection.</param>
    /// <typeparam name="T">The type of the collection.</typeparam>
    /// <returns>true if the collection was found</returns>
    public bool TryGetCollection<T>(byte collectionId, out DatabaseCollection<T> collection)
    {
        if (!collections.TryGetValue(collectionId, out var collectionBase)
            || collectionBase is not DatabaseCollection<T> collectionResult)
        {
            collection = default;
            return false;
        }
        
        collection = collectionResult;
        return true;
    }

    /// <summary>
    /// Gets a collection by an ID.
    /// </summary>
    /// <param name="collectionId">The ID of the collection.</param>
    /// <typeparam name="T">The type of the collection.</typeparam>
    /// <returns>The found collection.</returns>
    /// <exception cref="Exception">Collection with that ID does not exist.</exception>
    public DatabaseCollection<T> GetCollection<T>(byte collectionId)
    {
        if (TryGetCollection<T>(collectionId, out var collection))
            return collection;
        
        throw new Exception($"Collection {collectionId} not found");
    }

    /// <summary>
    /// Finds or creates a new collection.
    /// </summary>
    /// <param name="collectionId">The ID of the collection.</param>
    /// <typeparam name="T">The type of the collection.</typeparam>
    /// <returns>Found or created collection instance.</returns>
    public DatabaseCollection<T> GetOrAddCollection<T>(byte collectionId)
    {
        if (TryGetCollection<T>(collectionId, out var collection))
            return collection;

        collection = new DatabaseCollection<T>();

        collection.Table = this;
        collection.Id = collectionId;
        collection.Type = typeof(T);
        
        collections.Add(collectionId, collection);
        
        collection.InternalInit();
        
        Added?.InvokeSafe(this, collection);
        
        AddCollectionRequest.SendRequest(Id, collectionId, typeof(T).AssemblyQualifiedName);
        return collection;
    }

    /// <summary>
    /// Drops a collection.
    /// </summary>
    /// <param name="collectionId">The ID of the collection.</param>
    public void DropCollection(byte collectionId)
    {
        if (collections.TryGetValue(collectionId, out var collection))
        {
            Dropped?.InvokeSafe(this, collection);
            
            collection.InternalDestroy();
            
            collections.Remove(collectionId);
            
            ClearCollectionRequest.SendRequest(Id, collectionId, true);
        }
    }

    /// <summary>
    /// Clears a collection.
    /// </summary>
    /// <param name="collectionId">The ID of the collection.</param>
    public void ClearCollection(byte collectionId)
    {
        if (collections.TryGetValue(collectionId, out var collection)
            && collection.Size > 0)
        {
            collection.InternalClear();
            
            Cleared?.InvokeSafe(this, collection);
            
            ClearCollectionRequest.SendRequest(Id, collectionId, false);
        }
    }

    internal void OnAdded(DatabaseCollectionBase collection)
    {
        Added?.InvokeSafe(this, collection);
    }

    internal void InternalDropCollection(DatabaseCollectionBase collection)
    {
        Dropped?.InvokeSafe(this, collection);
        
        collection.InternalDestroy();
        collection.Table = null;
        
        collections.Remove(collection.Id);
    }

    internal void InternalClearCollection(DatabaseCollectionBase collection)
    {
        if (collection.Size > 0)
        {
            collection.InternalClear();
            
            Cleared?.InvokeSafe(this, collection);
        }
    }
    
    internal void InternalClear()
    {
        foreach (var collection in collections)
            collection.Value.InternalDestroy();
        
        collections.Clear();
    }

    internal void InternalDestroy(bool isDrop = false)
    {
        if (collections != null)
        {
            if (isDrop && DatabaseDirector.tables.ContainsKey(Id))
                DatabaseDirector.OnDropped(this);
            
            foreach (var collection in collections)
            {
                if (isDrop)
                    Dropped?.InvokeSafe(this, collection.Value);
                
                collection.Value.InternalDestroy(isDrop);
            }

            DictionaryPool<byte, DatabaseCollectionBase>.Shared.Return(collections);
        }

        collections = null;
    }
}