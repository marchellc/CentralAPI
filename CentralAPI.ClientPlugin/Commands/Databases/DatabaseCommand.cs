using CentralAPI.ClientPlugin.Databases;
using CentralAPI.ClientPlugin.Network;

using LabExtended.Commands;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using LabExtended.Extensions;

namespace CentralAPI.ClientPlugin.Commands.Databases;

[Command("database", "Database management commands.","db")]
public class DatabaseCommand : CommandBase, IServerSideCommand
{
     [CommandOverload("Prints the status of the database.")]
     private void Status()
     {
          if (NetworkClient.Scp is null)
          {
               Fail("Network is DISCONNECTED.");
               return;
          }

          if (!DatabaseDirector.IsDownloaded)
          {
               if (DatabaseDirector.IsDownloading)
               {
                    Fail("Database is currently DOWNLOADING");
                    return;
               }

               Fail("Database is NOT DOWNLOADED.");
               return;
          }

          if (DatabaseDirector.tables.Count < 1)
          {
               Ok("Database is EMPTY.");
               return;
          }

          Ok(x =>
          {
               x.AppendLine($"Database is DOWNLOADED ({DatabaseDirector.tables.Count} table(s))");

               foreach (var table in DatabaseDirector.tables)
               {
                    x.AppendLine();
                    x.AppendLine($" -< Table {table.Key} ({table.Value.collections.Count} collection(s))");

                    foreach (var collection in table.Value.collections)
                    {
                         x.AppendLine(
                              $"   -> Collection {collection.Key} ({collection.Value.Size} items; {collection.Value.Type?.FullName ?? "null type!"})");
                    }
               }
          });
     }

     [CommandOverload( "addtable", "Adds a new table to the database.")]
     private void AddTable(
          [CommandParameter("TableId", "The ID of the table to add.")] byte tableId)
     {
          if (NetworkClient.Scp is null)
          {
               Fail("Network is DISCONNECTED.");
               return;
          }

          if (!DatabaseDirector.IsDownloaded)
          {
               if (DatabaseDirector.IsDownloading)
               {
                    Fail("Database is currently DOWNLOADING");
                    return;
               }
               
               Fail("Database is NOT DOWNLOADED.");
               return;
          }

          if (DatabaseDirector.tables.ContainsKey(tableId))
          {
               Fail($"Table '{tableId}' already exists.");
               return;
          }

          _ = DatabaseDirector.GetOrAddTable(tableId);
          
          Ok($"Created table '{tableId}'");
     }
     
     [CommandOverload( "addcollection", "Adds a new collection to the database.")]
     private void AddCollection(
          [CommandParameter("TableId", "The ID of the parent table.")] byte tableId,
          [CommandParameter("CollectionId", "The ID of the collection to add.")] byte collectionId,
          [CommandParameter("TypeName", "Full name of the type used for the collection.")] string typeName)
     {
          if (NetworkClient.Scp is null)
          {
               Fail("Network is DISCONNECTED.");
               return;
          }

          if (!DatabaseDirector.IsDownloaded)
          {
               if (DatabaseDirector.IsDownloading)
               {
                    Fail("Database is currently DOWNLOADING");
                    return;
               }
               
               Fail("Database is NOT DOWNLOADED.");
               return;
          }

          if (!DatabaseDirector.TryGetTable(tableId, out var table))
          {
               Fail($"Table '{tableId}' does not exist.");
               return;
          }

          if (table.collections.ContainsKey(collectionId))
          {
               Fail($"Collection '{collectionId}' already exists.");
               return;
          }

          var type = Type.GetType(typeName, false, true);

          if (type is null)
          {
               Fail($"Type '{typeName}' could not be found.");
               return;
          }

          var addMethod = typeof(DatabaseTable).FindMethod(x => x.Name == "GetOrAddCollection");

          if (addMethod is null)
          {
               Fail($"GetOrAddCollection method could not be found.");
               return;
          }
          
          addMethod = addMethod.MakeGenericMethod(type);

          _ = addMethod.Invoke(table, [collectionId]);
          
          Ok($"Added collection '{collectionId}'");
     }

     [CommandOverload("cleartable", "Clears (or drops) a database table.")]
     private void ClearTable(
          [CommandParameter("TableId", "The ID of the table to clear / drop.")] byte tableId, 
          [CommandParameter("DropTable", "Whether or not to drop the table (defaults to false).")] bool dropTable = false)
     {
          if (NetworkClient.Scp is null)
          {
               Fail("Network is DISCONNECTED.");
               return;
          }

          if (!DatabaseDirector.IsDownloaded)
          {
               if (DatabaseDirector.IsDownloading)
               {
                    Fail("Database is currently DOWNLOADING");
                    return;
               }
               
               Fail("Database is NOT DOWNLOADED.");
               return;
          }

          if (!DatabaseDirector.TryGetTable(tableId, out var table))
          {
               Fail($"Table '{tableId}' does not exist.");
               return;
          }

          if (dropTable)
          {
               DatabaseDirector.DropTable(tableId);
               
               Ok($"Table '{tableId}' dropped.");
          }
          else
          {
               DatabaseDirector.ClearTable(tableId);
               
               Ok($"Table '{tableId}' cleared.");
          }
     }

     [CommandOverload("clearcollection", "Clears (or drops) a database collection.")]
     private void ClearCollection(
          [CommandParameter("TableId", "The ID of the table that owns the collection.")] byte tableId, 
          [CommandParameter("CollectionId", "The ID of the collection to clear / drop.")] byte collectionId,
          [CommandParameter("DropCollection", "Whether or not to drop the collection (defaults to false).")] bool dropCollection = false)
     {
          if (NetworkClient.Scp is null)
          {
               Fail("Network is DISCONNECTED.");
               return;
          }

          if (!DatabaseDirector.IsDownloaded)
          {
               if (DatabaseDirector.IsDownloading)
               {
                    Fail("Database is currently DOWNLOADING");
                    return;
               }
               
               Fail("Database is NOT DOWNLOADED.");
               return;
          }

          if (!DatabaseDirector.TryGetTable(tableId, out var table))
          {
               Fail($"Table '{tableId}' does not exist.");
               return;
          }

          if (!table.collections.ContainsKey(collectionId))
          {
               Fail($"Collection '{collectionId}' does not exist.");
               return;
          }

          if (dropCollection)
          {
               table.DropCollection(collectionId);
               
               Ok($"Collection '{collectionId}' dropped.");
          }
          else
          {
               table.ClearCollection(collectionId);
               
               Ok($"Collection '{collectionId}' cleared.");
          }
     }

     [CommandOverload("find", "Finds a specific item.")]
     private void Find(
          [CommandParameter("TableId", "The ID of the table which owns the collection this item is in.")] byte tableId, 
          [CommandParameter("CollectionId", "The ID of the collection which contains this item.")] byte collectionId,
          [CommandParameter("ItemId", "The ID of the item.")] string itemId)
     {
          if (NetworkClient.Scp is null)
          {
               Fail("Network is DISCONNECTED.");
               return;
          }

          if (!DatabaseDirector.IsDownloaded)
          {
               if (DatabaseDirector.IsDownloading)
               {
                    Fail("Database is currently DOWNLOADING");
                    return;
               }
               
               Fail("Database is NOT DOWNLOADED.");
               return;
          }

          if (!DatabaseDirector.TryGetTable(tableId, out var table))
          {
               Fail($"Table '{tableId}' does not exist.");
               return;
          }

          if (!table.collections.TryGetValue(collectionId, out var collection))
          {
               Fail($"Collection '{collectionId}' does not exist.");
               return;
          }

          if (!collection.InternalTryGetString(itemId, out var value))
          {
               Fail($"Item '{itemId}' could not be found (or converted).");
               return;
          }
          
          Ok($"Item '{itemId}' found:\n{value}");
     }

     [CommandOverload("download", "Re-downloads the whole database.")]
     private void Download()
     {
          DatabaseDirector.Download();
          
          Ok("Started database download.");
     }
}