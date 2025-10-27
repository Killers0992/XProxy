namespace XProxy.API.Misc;

public class ProxyLogger
{
    public static ConcurrentQueue<string> NewLogEntry = new ConcurrentQueue<string>();

    public static bool AnsiDisabled { get; set; }
    public static bool DebugMode { get; set; }

    public static DateTime SessionTime = DateTime.Now;

    static string TimeString => DateTime.Now.TimeOfDay
        .ToString("hh\\:mm\\:ss")
        .ToString();

    public static void Info(object message, string tag = "XProxy") => WriteLine($" (f=darkgray){TimeString}(f=white) [(f=cyan)INFO(f=white)] {(tag != null ? $"[(f=magenta){tag}(f=white)] " : string.Empty)}{message}");
    public static void Error(object message, string tag = "XProxy") => WriteLine($" (f=darkgray){TimeString}(f=white) [(f=darkred)ERROR(f=white)] {(tag != null ? $"[(f=magenta){tag}(f=white)] " : string.Empty)}(f=red){message}");
    public static void Warn(object message, string tag = "XProxy") => WriteLine($" (f=darkgray){TimeString}(f=white) [(f=darkyellow)WARN(f=white)] {(tag != null ? $"[(f=magenta){tag}(f=white)] " : string.Empty)}(f=yellow){message}");
    
    public static void Debug(object message, string tag = "XProxy")
    {
        if (!DebugMode)
            return;

        WriteLine($" (f=darkgray){TimeString}(f=white) [(f=yellow)DEBUG(f=white)] {(tag != null ? $"[(f=magenta){tag}(f=white)] " : string.Empty)}(f=yellow){message}");
    }

    static void WriteLine(object message)
    {
        NewLogEntry.Enqueue(message.ToString());
    }
}
