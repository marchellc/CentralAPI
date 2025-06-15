using CentralAPI.ClientPlugin.Databases.Internal;

using LabExtended.Core;

using NetworkLib;

using NetworkClient = CentralAPI.ClientPlugin.Network.NetworkClient;

namespace CentralAPI.ClientPlugin.Databases.Requests;

internal static class DownloadRequest
{
    private static ushort code;
    
    internal static void SendRequest()
    {
        DatabaseDirector.OnDownloading();
        
        NetworkClient.Scp.Request(code, null, HandleResponse);
    }
    
    internal static void HandleResponse(NetworkReader reader)
    {
        var count = reader.ReadByte();

        ApiLog.Debug("Database Director", $"&3[DownloadRequest]&r Received size: {count}");
        
        for (var i = 0; i < count; i++)
        {
            var tableId = reader.ReadByte();
            var tableCount = reader.ReadByte();
            var table = new DatabaseTable();

            table.Id = tableId;
            
            ApiLog.Debug("Database Director", $"&3[DownloadRequest]&r Received table: ID={tableId}; Count={tableCount}");
            
            DatabaseDirector.tables.Add(tableId, table);

            for (var x = 0; x < tableCount; x++)
            {
                var collectionId = reader.ReadByte();
                var collectionCount = reader.ReadInt();
                var collectionTypeName = reader.ReadString();
                var collectionType = Type.GetType(collectionTypeName, true);
                var collection = Activator.CreateInstance(typeof(DatabaseCollection<>).MakeGenericType(collectionType)) as DatabaseCollectionBase;
                
                var itemType = typeof(DatabaseItem<>).MakeGenericType(collectionType);

                collection.Table = table;
                
                collection.Id = collectionId;
                collection.Type = collectionType;
                
                ApiLog.Debug("Database Director", $"&3[DownloadRequest]&r Received collection: ID={collectionId}; Count={collectionCount}; Type={collectionType.FullName}");
                
                table.collections.Add(collectionId, collection);

                collection.InternalInit();
                
                table.OnAdded(collection);
                
                for (var y = 0; y < collectionCount; y++)
                {
                    var itemName = reader.ReadString();
                    var itemValue = reader.ReadReader();
                    var item = Activator.CreateInstance(itemType) as DatabaseItemBase;

                    item.Name = itemName;

                    item.reader = itemValue;
                    item.collection = collection;
                    item.type = collectionType;
                    
                    collection.InternalAddItem(item);
                }
                
                DatabaseDirector.OnAdded(table);
            }
        }

        DatabaseDirector.OnDownloaded();
    }

    internal static void Init()
    {
        code = NetworkClient.scp.GetRequestType("Database.Download");
    }
}