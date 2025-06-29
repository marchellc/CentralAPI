using NetworkLib;
using NetworkLib.Interfaces;

namespace CentralAPI.SharedLib.Punishments.Warns;

/// <summary>
/// Message sent to servers when a new warn is created.
/// </summary>
public struct ConfirmedWarnMessage : INetworkMessage
{
    /// <summary>
    /// The assigned warn ID.
    /// </summary>
    public ulong Id;

    /// <summary>
    /// The server transaction ID.
    /// </summary>
    public int TransactionId;

    /// <summary>
    /// Whether or not the transaction is a warn removal.
    /// </summary>
    public bool IsRemoval;

    /// <summary>
    /// Creates a new <see cref="ConfirmedWarnMessage"/> instance.
    /// </summary>
    /// <param name="id">The assigned warn ID.</param>
    /// <param name="transactionId">The server transaction ID.</param>
    /// <param name="isRemoval">Whether or not the transaction is a warn removal.</param>
    public ConfirmedWarnMessage(ulong id, int transactionId, bool isRemoval)
    {
        Id = id;
        TransactionId = transactionId;
        IsRemoval = isRemoval;
    }

    /// <inheritdoc cref="INetworkMessage.Read"/>>
    public void Read(NetworkReader reader)
    {
        IsRemoval = reader.ReadBool();
        
        if (IsRemoval)
            Id = reader.ReadULong();
        else
            TransactionId = reader.ReadInt();
    }

    /// <inheritdoc cref="INetworkMessage.Write"/>>
    public void Write(NetworkWriter writer)
    {
        writer.WriteBool(IsRemoval);
        
        if (IsRemoval)
            writer.WriteULong(Id);
        else
            writer.WriteInt(TransactionId);
    }
}