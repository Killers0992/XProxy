using LiteNetLib;
using LiteNetLib.Utils;
using Mirror;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using XProxy;
using XProxy.Core.Events;
using static XProxy.Core.Events.EventManager;

public static class Extensions
{
    public static byte[] ReadByteArray(this NetworkReader reader)
    {
        int size = reader.ReadInt();
        byte[] data = new byte[size];

        for (int x = 0; x < size; x++)
        {
            data[x] = reader.ReadByte();
        }
        return data;
    }

    public static void WriteByteArray(this NetworkWriter writer, byte[] array)
    {
        writer.WriteInt(array.Length);
        for (int x = 0; x < array.Length; x++)
        {
            writer.WriteByte(array[x]);
        }
    }

    public static void WriteArraySegment(this NetworkWriter writer, ArraySegment<byte> array)
    {
        if (array == null)
        {
            writer.WriteUInt(0U);
            return;
        }
        writer.WriteUInt(checked((uint)array.Count) + 1U);
        writer.WriteBytes(array.Array, array.Offset, array.Count);
    }

    public static void Disconnect(this ConnectionRequest request, string message)
    {
        NetDataWriter writer = new NetDataWriter();
        writer.Put((byte)RejectionReason.Custom);
        writer.Put(message);
        request.Reject(writer);
    }

    public static void DisconnectBanned(this ConnectionRequest request, string reason, long expiration)
    {
        NetDataWriter writer = new NetDataWriter();
        writer.Put((byte)RejectionReason.Banned);
        writer.Put(expiration);
        writer.Put(reason);
        request.Reject(writer);
    }

    public static void DisconnectBanned(this ConnectionRequest request, string reason, DateTime date) => request.DisconnectBanned(reason, date.Ticks);

    public static void DisconnectServerFull(this ConnectionRequest request)
    {
        NetDataWriter writer = new NetDataWriter();
        writer.Put((byte)RejectionReason.ServerFull);
        request.Reject(writer);
    }

    public static void DisconnectWrongVersion(this ConnectionRequest request)
    {
        NetDataWriter writer = new NetDataWriter();
        writer.Put((byte)RejectionReason.VersionMismatch);
        request.Reject(writer);
    }

    public static string ToReadableString(this TimeSpan span)
    {
        string formatted = string.Format("{0}{1}{2}{3}",
            span.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? string.Empty : "s") : string.Empty,
            span.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? string.Empty : "s") : string.Empty,
            span.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", span.Minutes, span.Minutes == 1 ? string.Empty : "s") : string.Empty,
            span.Duration().Seconds > 0 ? string.Format("{0:0} second{1}", span.Seconds, span.Seconds == 1 ? string.Empty : "s") : string.Empty);

        if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

        if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

        return formatted;
    }

    public static void InvokeWithExceptionHandler<TEvent>(this CustomEventHandler<TEvent> ev, TEvent arguments) where TEvent: BaseEvent
    {
        foreach(var invoker in ev.GetInvocationList())
        {
            if (invoker is not CustomEventHandler<TEvent> customInvoker) continue;

            try
            {
                customInvoker.Invoke(arguments);
            }
            catch (Exception ex)
            {
                Logger.Error($"Exception while invoking event (f=green){ev.GetType().Name}(f=red) {ex}", "EventManager");
            }
        }
    }

    public static async Task DownloadAsync(this HttpClient client, string requestUri, Stream destination, IProgress<float> progress = null, CancellationToken cancellationToken = default)
    {
        using (var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead))
        {
            var contentLength = response.Content.Headers.ContentLength;

            using (var download = await response.Content.ReadAsStreamAsync(cancellationToken))
            {
                if (progress == null || !contentLength.HasValue)
                {
                    await download.CopyToAsync(destination);
                    return;
                }

                var relativeProgress = new Progress<long>(totalBytes => progress.Report((float)totalBytes / contentLength.Value));
                await download.CopyToAsync(destination, 81920, relativeProgress, cancellationToken);
                progress.Report(1);
            }
        }
    }

    public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<long> progress = null, CancellationToken cancellationToken = default)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        if (!source.CanRead)
            throw new ArgumentException("Has to be readable", nameof(source));
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));
        if (!destination.CanWrite)
            throw new ArgumentException("Has to be writable", nameof(destination));
        if (bufferSize < 0)
            throw new ArgumentOutOfRangeException(nameof(bufferSize));

        var buffer = new byte[bufferSize];
        long totalBytesRead = 0;
        int bytesRead;
        while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
        {
            await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
            totalBytesRead += bytesRead;
            progress?.Report(totalBytesRead);
        }
    }
}