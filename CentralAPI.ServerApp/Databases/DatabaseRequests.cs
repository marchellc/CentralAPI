using CentralAPI.ServerApp.Databases.Requests;
using CentralAPI.ServerApp.Server;

namespace CentralAPI.ServerApp.Databases;

/// <summary>
/// Handles database requests.
/// </summary>
public static class DatabaseRequests
{
    private static volatile bool codesCreated;
    
    internal static void Register(ScpInstance instance)
    {
        if (instance != null)
        {
            if (!codesCreated)
            {
                RemoveItemRequest.CreateCodes(instance);
                AddItemRequest.CreateCodes(instance);
                GetItemRequest.CreateCodes(instance);
                ClearCollectionRequest.CreateCodes(instance);
                ClearTableRequest.CreateCodes(instance);
                AddTableRequest.CreateCodes(instance);
                AddCollectionRequest.CreateCodes(instance);

                codesCreated = true;
            }

            instance.HandleRequest("Database.RemoveItem", (reader, writer) => RemoveItemRequest.Handle(instance, reader, writer));
            instance.HandleRequest("Database.AddItem", (reader, writer) => AddItemRequest.Handle(instance, reader, writer));
            instance.HandleRequest("Database.EnsureExistence", (reader, writer) => EnsureRequest.Handle(instance, reader, writer));
            instance.HandleRequest("Database.GetItem", (reader, writer) => GetItemRequest.Handle(instance, reader, writer));
            instance.HandleRequest("Database.ClearCollection", (reader, writer) => ClearCollectionRequest.Handle(instance, reader, writer));
            instance.HandleRequest("Database.ClearTable", (reader, writer) => ClearTableRequest.Handle(instance, reader, writer));
            instance.HandleRequest("Database.Download", (reader, writer) => DownloadRequest.Handle(instance, reader, writer));
            instance.HandleRequest("Database.AddTable", (reader, writer) => AddTableRequest.Handle(instance, reader, writer));
            instance.HandleRequest("Database.AddCollection", (reader, writer) => AddCollectionRequest.Handle(instance, reader, writer));
        }
    }
}