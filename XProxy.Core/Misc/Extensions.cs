using LiteNetLib;
using LiteNetLib.Utils;
using Mirror;
using System;
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
}