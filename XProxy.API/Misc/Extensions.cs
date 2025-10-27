using System.Buffers;
using System.Text;
using System.Text.RegularExpressions;

namespace XProxy.API.Misc;

public static class Extensions
{
    public const char ESC = (char)27;

    public static bool ValidateGameVersion(this Version gameVersion, Version clientVersion, bool backwardsCompatible, byte backwardsRevision)
    {
        if (gameVersion.Major != clientVersion.Major || gameVersion.Minor != clientVersion.Minor)
            return false;

        if (backwardsCompatible)
            return gameVersion.Build == backwardsRevision;

        return gameVersion.Build >= backwardsRevision && gameVersion.Build <= clientVersion.Build;
    }

    public static void RejectWithMessage(this ConnectionRequest request, string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            request.Reject();
            return;
        }

        NetDataWriter writer = new NetDataWriter();

        writer.Put((byte)RejectionReason.Custom);
        writer.Put(message);

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

    public static string FormatAnsi(this object message, bool forceRemove = false)
    {
        string text = message.ToString();

        return Regex.Replace(text, @"\(f=(.*?)\)", ev =>
        {
            if (ProxyLogger.AnsiDisabled || forceRemove)
                return string.Empty;

            string color = ev.Groups[1].Value.ToLower();

            switch (color)
            {
                case "black":
                    return $"{ESC}[30m";

                case "darkred":
                    return $"{ESC}[31m";
                case "darkgreen":
                    return $"{ESC}[32m";
                case "darkyellow":
                    return $"{ESC}[33m";
                case "darkblue":
                    return $"{ESC}[34m";
                case "darkmagenta":
                    return $"{ESC}[35m";
                case "darkcyan":
                    return $"{ESC}[36m";
                case "darkgray":
                    return $"{ESC}[90m";

                case "gray":
                    return $"{ESC}[37m";
                case "red":
                    return $"{ESC}[91m";
                case "green":
                    return $"{ESC}[92m";
                case "yellow":
                    return $"{ESC}[93m";
                case "blue":
                    return $"{ESC}[94m";
                case "magenta":
                    return $"{ESC}[95m";
                case "cyan":
                    return $"{ESC}[96m";

                case "white":
                    return $"{ESC}[97m";

                default:
                    return $"{ESC}[39m";
            }
        });
    }

    public static string Base64Decode(this string base64EncodedData)
    {
        byte[] bytes = Convert.FromBase64String(base64EncodedData);

        return Encoding.UTF8.GetString(bytes);
    }

    public static string Base64Encode(this string text)
    {
        byte[] array = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(text.Length));

        int bytes = Encoding.UTF8.GetBytes(text, 0, text.Length, array, 0);

        string result = Convert.ToBase64String(array, 0, bytes);

        ArrayPool<byte>.Shared.Return(array, false);

        return result;
    }

}
