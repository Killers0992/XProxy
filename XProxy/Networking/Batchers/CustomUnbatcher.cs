namespace XProxy.Networking.Batchers;

public class CustomUnbatcher
{
    readonly Queue<NetworkWriter> batches = new Queue<NetworkWriter>();

    public int BatchesCount => batches.Count;

    readonly NetworkReader reader = new NetworkReader(new byte[0]);

    double readerRemoteTimeStamp;

    void StartReadingBatch(NetworkWriter batch)
    {
        reader.SetBuffer(batch.ToArraySegment());

        readerRemoteTimeStamp = reader.ReadDouble();
    }

    public bool AddBatch(ArraySegment<byte> batch)
    {
        if (batch.Count < Batcher.TimestampSize)
            return false;

        NetworkWriter writer = new NetworkWriter();
        writer.WriteBytes(batch.Array, batch.Offset, batch.Count);

        if (batches.Count == 0)
            StartReadingBatch(writer);

        batches.Enqueue(writer);
        return true;
    }

    public bool GetNextMessage(out ArraySegment<byte> message, out double remoteTimeStamp)
    {
        message = default;
        remoteTimeStamp = 0;

        if (batches.Count == 0)
            return false;

        if (reader.Capacity == 0)
            return false;

        if (reader.Remaining == 0)
        {
            batches.Dequeue();

            if (batches.Count > 0)
            {
                NetworkWriter next = batches.Peek();
                StartReadingBatch(next);
            }
            else return false;
        }

        remoteTimeStamp = readerRemoteTimeStamp;

        if (reader.Remaining == 0)
            return false;

        int size = (int)Compression.DecompressVarUInt(reader);

        if (reader.Remaining < size)
            return false;

        message = reader.ReadBytesSegment(size);
        return true;
    }
}
