namespace Lobby.Models;

public class PortalInfo
{
    public string TargetServer { get; set; }

    public string Text { get; set; } = $"<size=5>Server\n<color=orange>%serverName%</color></size>";

    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }

    public float Rotation { get; set; }
}
