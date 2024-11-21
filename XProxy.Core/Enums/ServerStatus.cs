namespace XProxy.Core.Enums
{
    public enum ServerStatus : byte
    {
        Starting,
        WaitingForPlayers,
        StartingRound,
        RoundInProgress,
        RoundEnding,
        RoundRestart,
    }
}
