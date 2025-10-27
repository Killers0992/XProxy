using LiteNetLib;

namespace XProxy.Misc;

public class CustomNetLogger : INetLogger
{
    private const string _tag = "LiteNetLib";

    public void WriteNet(NetLogLevel level, string str, params object[] args)
    {
        string text = string.Format(str, args);
        switch (level)
        {
            case NetLogLevel.Error:
                ProxyLogger.Error(text, _tag);
                break;

            case NetLogLevel.Trace:
            case NetLogLevel.Warning:
                ProxyLogger.Warn(text, _tag);
                break;

            case NetLogLevel.Info:
                ProxyLogger.Info(text, _tag);
                break;
        }
    }
}
