namespace XProxy;

public static class Extensions
{
    public static bool ValidateGameVersion(this Version gameVersion, Version clientVersion, bool backwardsCompatible, byte backwardsRevision)
    {
        if (gameVersion.Major != clientVersion.Major || gameVersion.Minor != clientVersion.Minor)
            return false;

        if (!backwardsCompatible)
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
}
