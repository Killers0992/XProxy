using System.ComponentModel;

namespace XProxy.Patcher.Models
{
    public class PatcherConfig
    {
        [Description("For which version of game Core will be downloaded.")]
        public string GameVersion { get; set; } = "13.5.1";
        [Description("Console logs will contain colors.")]
        public bool AnsiColors { get; set; } = true;
    }
}
