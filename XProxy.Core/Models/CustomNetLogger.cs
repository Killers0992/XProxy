using LiteNetLib;

namespace XProxy.Core.Models
{
    public class CustomNetLogger : INetLogger
    {
        private const string _tag = "LiteNetLib";

        public void WriteNet(NetLogLevel level, string str, params object[] args)
        {
            string text = string.Format(str, args);
            switch (level)
            {
                case NetLogLevel.Error:
                    Logger.Error(text, _tag);
                    break;
                case NetLogLevel.Trace:
                case NetLogLevel.Warning:
                    Logger.Warn(text, _tag);
                    break;
                case NetLogLevel.Info:
                    Logger.Info(text, _tag);
                    break;
            }
        }
    }
}
