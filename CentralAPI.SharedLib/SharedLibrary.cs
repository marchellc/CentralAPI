using CentralAPI.SharedLib.PlayerProfiles;
using CentralAPI.SharedLib.Requests;

using NetworkLib;
using NetworkLib.Pools;

namespace CentralAPI.SharedLib;

/// <summary>
/// Shared library functions.
/// </summary>
public static class SharedLibrary
{
    /// <summary>
    /// Reads a <see cref="DateTime"/> instance.
    /// </summary>
    /// <param name="reader">The target reader.</param>
    /// <returns>The read DateTime instance.</returns>
    public static DateTime ReadDate(this NetworkReader reader)
    {
        return new(
            (int)reader.ReadShort(), 
            (int)reader.ReadByte(), 
            (int)reader.ReadByte(), 
            (int)reader.ReadByte(), 
            (int)reader.ReadByte(), 
            (int)reader.ReadByte());
    }

    /// <summary>
    /// Reads a <see cref="TimeSpan"/> instance.
    /// </summary>
    /// <param name="reader">The target reader.</param>
    /// <returns>The read TimeSpan instance.</returns>
    public static TimeSpan ReadTime(this NetworkReader reader)
    {
        return TimeSpan.FromMilliseconds(reader.ReadDouble());
    }

    /// <summary>
    /// Writes a <see cref="DateTime"/> instance.
    /// </summary>
    /// <param name="writer">The target writer.</param>
    /// <param name="date">The date to write.</param>
    public static void WriteDate(this NetworkWriter writer, DateTime date)
    {
        writer.WriteUShort((ushort)date.Year);
        writer.WriteByte((byte)date.Month);
        writer.WriteByte((byte)date.Day);
        writer.WriteByte((byte)date.Hour);
        writer.WriteByte((byte)date.Minute);
        writer.WriteByte((byte)date.Second);
    }

    /// <summary>
    /// Writes a <see cref="TimeSpan"/> instance.
    /// </summary>
    /// <param name="writer">The target writer.</param>
    /// <param name="time">The time to write.</param>
    public static void WriteTime(this NetworkWriter writer, TimeSpan time)
    {
        writer.WriteDouble(time.TotalMilliseconds);
    }
    
    /// <summary>
    /// Executes a writer delegate.
    /// </summary>
    public static byte[] WriteAction(Action<NetworkWriter> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        var writer = NetworkDataPool.GetWriter();
        
        var result = default(byte[]);
        var exception = default(Exception);

        try
        {
            action(writer);

            result = writer.Buffer.ToArray();
        }
        catch (Exception ex)
        {
            exception = ex;
        }
        finally
        {
            NetworkDataPool.Return(writer);
        }

        if (exception != null)
            throw exception;

        return result;
    }
    
    /// <summary>
    /// Executes a reader delegate.
    /// </summary>
    public static void ReadAction(this byte[] data, Action<NetworkReader> action)
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));
        
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        var reader = NetworkDataPool.ReaderPool.TryDequeue(out var r)
            ? r
            : new();
        
        reader.Reset();
        reader.Buffer.AddRange(data);

        try
        {
            action(reader);
        }
        finally
        {
            NetworkDataPool.Return(reader);
        }
    }
    
    /// <summary>
    /// Registers all custom message types.
    /// </summary>
    public static void RegisterMessages()
    {
        NetworkLibrary.CollectMessageTypes();
        NetworkLibrary.MessageTypes = NetworkLibrary.MessageTypes
            .Concat([
                typeof(RequestMessage), 
                typeof(ResponseMessage), 
                
                typeof(PlayerProfilePackageMessage),
                typeof(PlayerProfileUpdateMessage)])
            .ToArray();
    }
}