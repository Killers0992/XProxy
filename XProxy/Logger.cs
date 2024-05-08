using AnsiVtConsole.NetCore;
using System;
using System.Drawing;
using XProxy.Services;

namespace XProxy
{
    public class Logger
    {
        public static AnsiVtConsole.NetCore.AnsiVtConsole Ansi { get; set; }

        static string TimeString => DateTime.Now.TimeOfDay
            .ToString("hh\\:mm\\:ss")
            .ToString();

        public static void Info(object message, string tag = null) => Ansi.Out.WriteLine($" (f=darkgray){TimeString}(f=white) [(f=cyan)INFO(f=white)] {(tag != null ? $"[(f=magenta){tag}(f=white)] " : string.Empty)}{message}");
        public static void Error(object message, string tag = null) => Ansi.Out.WriteLine($" (f=darkgray){TimeString}(f=white) [(f=darkred)ERROR(f=white)] {(tag != null ? $"[(f=magenta){tag}(f=white)] " : string.Empty)}(f=red){message}");
        public static void Warn(object message, string tag = null) => Ansi.Out.WriteLine($" (f=darkgray){TimeString}(f=white) [(f=darkyellow)WARN(f=white)] {(tag != null ? $"[(f=magenta){tag}(f=white)] " : string.Empty)}(f=yellow){message}");
        public static void Debug(object message, string tag = null)
        {
            if (ConfigService.Instance.Value.Debug)
                Ansi.Out.WriteLine($" (f=darkgray){TimeString}(f=white) [(f=yellow)DEBUG(f=white)] {(tag != null ? $"[(f=magenta){tag}(f=white)] " : string.Empty)}(f=yellow){message}");
        }
    }
}
