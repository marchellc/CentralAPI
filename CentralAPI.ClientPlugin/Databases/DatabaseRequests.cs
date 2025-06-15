using CentralAPI.ClientPlugin.Databases.Requests;

namespace CentralAPI.ClientPlugin.Databases;

/// <summary>
/// Handles requests from the server.
/// </summary>
public static class DatabaseRequests
{
    private static bool hasInit;
    
    internal static void OnReady()
    {
        if (!hasInit)
        {
            EnsureRequest.Init();
            DownloadRequest.Init();
            
            hasInit = true;
        }
        
        AddItemRequest.Init();
        ClearCollectionRequest.Init();
        ClearTableRequest.Init();
        RemoveItemRequest.Init();
        AddTableRequest.Init();
        AddCollectionRequest.Init();
    }
}