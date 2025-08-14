using System;
using System.Collections.Concurrent;
using System.Threading;

namespace XProxy.Networking.Batchers
{
    /// <summary>
    /// Collects messages into batches for efficient network sending.
    /// Allows multiple threads to add messages concurrently with no locks,
    /// while only one thread retrieves batches.
    /// </summary>
    public class CustomBatcher
    {
        private readonly int threshold;
        private readonly ConcurrentQueue<NetworkWriter> batches = new ConcurrentQueue<NetworkWriter>();
        private NetworkWriter batch; // shared current batch

        /// <summary>
        /// The size (in bytes) of the timestamp included in each batch.
        /// </summary>
        public const int TimestampSize = sizeof(double);

        /// <summary>
        /// Calculates the number of bytes required to store a message size as a VarUInt.
        /// </summary>
        public static int MessageHeaderSize(int messageSize) =>
            Compression.VarUIntSize((ulong)messageSize);

        /// <summary>
        /// Calculates the maximum possible overhead (timestamp + length header) for a message.
        /// </summary>
        public static int MaxMessageOverhead(int messageSize) =>
            TimestampSize + MessageHeaderSize(messageSize);

        /// <summary>
        /// Initializes a new instance of <see cref="CustomBatcher"/>.
        /// </summary>
        public CustomBatcher(int threshold)
        {
            this.threshold = threshold;
        }

        /// <summary>
        /// Adds a message to the current batch.
        /// If the batch exceeds the threshold, it is enqueued for retrieval.
        /// Thread-safe for concurrent calls without locks.
        /// </summary>
        public void AddMessage(ArraySegment<byte> message, double timeStamp)
        {
            if (message == null)
                return;

            int headerSize = Compression.VarUIntSize((ulong)message.Count);
            int neededSize = headerSize + message.Count;

            while (true)
            {
                NetworkWriter current = Volatile.Read(ref batch);

                if (current != null &&
                    current.Position + neededSize > threshold)
                {
                    // Batch is full — try to replace it atomically
                    if (Interlocked.CompareExchange(ref batch, null, current) == current)
                    {
                        batches.Enqueue(current);
                        current = null;
                    }
                    else
                    {
                        // Another thread swapped it first, retry
                        continue;
                    }
                }

                if (current == null)
                {
                    var newBatch = new NetworkWriter();
                    newBatch.WriteDouble(timeStamp);

                    // Try to set as current batch
                    if (Interlocked.CompareExchange(ref batch, newBatch, null) == null)
                        current = newBatch;
                    else
                        continue; // Another thread set batch first, retry
                }

                // Write message
                Compression.CompressVarUInt(current, (ulong)message.Count);
                current.WriteBytes(message.Array, message.Offset, message.Count);
                break;
            }
        }

        /// <summary>
        /// Copies the data from one writer to another and ensures the target is empty.
        /// </summary>
        private static void CopyAndReturn(NetworkWriter batch, NetworkWriter writer)
        {
            if (writer.Position != 0)
                throw new ArgumentException("GetBatch requires a fresh (empty) writer.");

            ArraySegment<byte> segment = batch.ToArraySegment();
            writer.WriteBytes(segment.Array, segment.Offset, segment.Count);
        }

        /// <summary>
        /// Retrieves the next available batch for sending.
        /// Only one thread should call this method at a time.
        /// </summary>
        public bool GetBatch(NetworkWriter writer)
        {
            // First, try to get from completed batches
            if (batches.TryDequeue(out NetworkWriter queued))
            {
                CopyAndReturn(queued, writer);
                return true;
            }

            // If nothing queued, take current batch
            NetworkWriter current = Interlocked.Exchange(ref batch, null);
            if (current != null)
            {
                CopyAndReturn(current, writer);
                return true;
            }

            return false;
        }
    }
}