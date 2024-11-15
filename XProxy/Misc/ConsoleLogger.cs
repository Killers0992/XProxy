using System.Text.RegularExpressions;

namespace XProxy.Misc
{
    public class ConsoleLogger
    {
        public static bool AnsiDisabled { get; set; }

        public const char ESC = (char)27;

        public static string TimeString => DateTime.Now.TimeOfDay
            .ToString("hh\\:mm\\:ss")
            .ToString();

        public static void Info(object message, string tag = null) => WriteLine($" (f=darkgray){TimeString}(f=white) [(f=cyan)INFO(f=white)] {(tag != null ? $"[(f=magenta){tag}(f=white)] " : string.Empty)}{message}");
        public static void Error(object message, string tag = null) => WriteLine($" (f=darkgray){TimeString}(f=white) [(f=darkred)ERROR(f=white)] {(tag != null ? $"[(f=magenta){tag}(f=white)] " : string.Empty)}(f=red){message}");
        public static void Warn(object message, string tag = null) => WriteLine($" (f=darkgray){TimeString}(f=white) [(f=darkyellow)WARN(f=white)] {(tag != null ? $"[(f=magenta){tag}(f=white)] " : string.Empty)}(f=yellow){message}");

        static void WriteLine(object message) => Console.WriteLine(FormatAnsi(message));

        public static string FormatAnsi(object message, bool forceRemove = false)
        {
            string text = message.ToString();

            return Regex.Replace(text, @"\(f=(.*?)\)", ev =>
            {
                if (AnsiDisabled || forceRemove)
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
    }
}
