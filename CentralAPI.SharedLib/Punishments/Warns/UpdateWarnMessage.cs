using NetworkLib;
using NetworkLib.Interfaces;

namespace CentralAPI.SharedLib.Punishments.Warns;

/// <summary>
/// Used to create a new warn.
/// </summary>
public struct UpdateWarnMessage : INetworkMessage
{
    /// <summary>
    /// The warn data.
    /// </summary>
    public byte[] Data;

    /// <summary>
    /// The ID of the transaction.
    /// </summary>
    public int Id;

    /// <summary>
    /// The ID of the warn.
    /// </summary>
    public ulong WarnId;

    /// <summary>
    /// Whether or not the request is a transaction.
    /// </summary>
    public bool IsTransaction;

    /// <summary>
    /// Creates a new <see cref="UpdateWarnMessage"/> instance.
    /// </summary>
    /// <param name="data">The warn data.</param>
    /// <param name="transactionId">The transaction ID.</param>
    /// <param name="warnId">The warn ID.</param>
    /// <param name="isTransaction">Whether or not the request is a transaction.</param>
    public UpdateWarnMessage(byte[] data, int transactionId, ulong warnId, bool isTransaction)
    {
        Data = data;
        Id = transactionId;
        WarnId = warnId;
        IsTransaction = isTransaction;
    }

    /// <inheritdoc cref="INetworkMessage.Read"/>>
    public void Read(NetworkReader reader)
    {
        IsTransaction = reader.ReadBool();

        if (IsTransaction)
            Id = reader.ReadInt();
        else
            WarnId = reader.ReadULong();

        Data = reader.ReadBytes();
    }

    /// <inheritdoc cref="INetworkMessage.Write"/>>
    public void Write(NetworkWriter writer)
    {
        writer.WriteBool(IsTransaction);
        
        if (IsTransaction)
            writer.WriteInt(Id);
        else
            writer.WriteULong(WarnId);
        
        writer.WriteBytes(Data);
    }
}