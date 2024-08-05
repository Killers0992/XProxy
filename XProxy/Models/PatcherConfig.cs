using System.ComponentModel;

namespace XProxy.Patcher.Models
{
    public class PatcherConfig
    {
        [Description("For which game version proxy will download builds of XProxy.")]
        public string GameVersion { get; set; } = "13.5.1";

        [Description("If console logs should contain ANSI colors.")]
        public bool AnsiColors { get; set; } = true;
    }
}
