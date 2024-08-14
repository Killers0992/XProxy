using Mirror;
using System;
using System.Collections.Generic;

public class CustomBatcher
{
    readonly int threshold;

    public const int TimestampSize = sizeof(double);

    public static int MessageHeaderSize(int messageSize) =>
        Compression.VarUIntSize((ulong)messageSize);

    public static int MaxMessageOverhead(int messageSize) =>
        TimestampSize + MessageHeaderSize(messageSize);

    readonly Queue<NetworkWriter> batches = new Queue<NetworkWriter>();

    NetworkWriter batch;

    public CustomBatcher(int threshold)
    {
        this.threshold = threshold;
    }

    public void AddMessage(ArraySegment<byte> message, double timeStamp)
    {
        int headerSize = Compression.VarUIntSize((ulong)message.Count);
        int neededSize = headerSize + message.Count;

        if (batch != null &&
            batch.Position + neededSize > threshold)
        {
            batches.Enqueue(batch);
            batch = null;
        }

        if (batch == null)
        {
            batch = new NetworkWriter();
            batch.WriteDouble(timeStamp);
        }

        Compression.CompressVarUInt(batch, (ulong)message.Count);
        batch.WriteBytes(message.Array, message.Offset, message.Count);
    }

    static void CopyAndReturn(NetworkWriter batch, NetworkWriter writer)
    {
        if (writer.Position != 0)
            throw new ArgumentException($"GetBatch needs a fresh writer!");

        ArraySegment<byte> segment = batch.ToArraySegment();
        writer.WriteBytes(segment.Array, segment.Offset, segment.Count);
    }

    public bool GetBatch(NetworkWriter writer)
    {
        if (batches.TryDequeue(out NetworkWriter first))
        {
            CopyAndReturn(first, writer);
            return true;
        }

        if (batch != null)
        {
            CopyAndReturn(batch, writer);
            batch = null;
            return true;
        }

        return false;
    }
}