using XProxy.Networking;

namespace XProxy.Models;

public struct PreAuth
{
    public Version ClientVersion;
    public bool BackwardCompatibility;
    public byte BackwardRevision;

    public string UserId;

    public long Expiration;

    public CentralAuthPreauthFlags CentralFlags;

    public string Country;

    public byte[] Signature;

    public string IpAddress;

    public PreAuth(Version clientVersion, bool backwardCompatibility, byte backwardRevision, string userId, long expiration, CentralAuthPreauthFlags centralFlags, string country, byte[] signature, string ipAddress)
    {
        ClientVersion = clientVersion;
        BackwardCompatibility = backwardCompatibility;
        BackwardRevision = backwardRevision;

        UserId = userId;
        
        Expiration = expiration;

        CentralFlags = centralFlags;

        Country = country;

        Signature = signature;

        IpAddress = ipAddress;
    }

    public NetDataWriter Create(bool includeIp, int challengeId = 0, byte[] challengeResponse = null)
    {
        NetDataWriter writer = new NetDataWriter();

        writer.Put((byte) ClientType.GameClient);
        writer.Put((byte) ClientVersion.Major);
        writer.Put((byte) ClientVersion.Minor);
        writer.Put((byte) ClientVersion.Build);
        writer.Put(BackwardCompatibility);

        if (BackwardCompatibility)
            writer.Put(BackwardRevision);

        writer.Put(challengeId);

        if (challengeId != 0)
            writer.PutBytesWithLength(challengeResponse);

        writer.Put(UserId);
        writer.Put(Expiration);
        writer.Put((byte)CentralFlags);
        writer.Put(Country);
        writer.PutBytesWithLength(Signature);

        if (includeIp)
            writer.Put(IpAddress);

        return writer;
    }

    public static bool TryRead(BaseListener listener, string connectionIp, NetDataReader reader, ref PreAuthResponse response, ref bool rejectForce, ref PreAuth preAuth)
    {
        if (!reader.TryGetByte(out byte rawClientType))
        {
            rejectForce = true;
            response = PreAuthResponse.InvalidClientType;
            return false;
        }

        if (!Enum.IsDefined(typeof(ClientType), rawClientType))
        {
            rejectForce = true;
            response = PreAuthResponse.ClientTypeOutOfRange;
            return false;
        }

        ClientType clientType = (ClientType)rawClientType;

        if (clientType == ClientType.VerificationService)
        {
            rejectForce = true;
            response = PreAuthResponse.ForbiddenClientType;
            return false;
        }

        if (!reader.TryGetByte(out byte major))
        {
            rejectForce = true;
            response = PreAuthResponse.InvalidMajorVersion;
            return false;
        }

        if (!reader.TryGetByte(out byte minor))
        {
            rejectForce = true;
            response = PreAuthResponse.InvalidMinorVersion;
            return false;
        }

        if (!reader.TryGetByte(out byte revision))
        {
            rejectForce = true;
            response = PreAuthResponse.InvalidRevisionVersion;
            return false;
        }

        if (!reader.TryGetBool(out bool backwardCompatibility))
        {
            rejectForce = true;
            response = PreAuthResponse.InvalidBackwardCompatibility;
            return false;
        }

        byte backwardRevision = 0;

        if (backwardCompatibility)
        {
            if (!reader.TryGetByte(out backwardRevision))
            {
                rejectForce = true;
                response = PreAuthResponse.InvalidBackwardRevision;
                return false;
            }
        }

        Version clientVersion = new Version(major, minor, revision);

        if (!listener.GameVersion.ValidateGameVersion(clientVersion, backwardCompatibility, backwardRevision))
        {
            response = PreAuthResponse.VersionNotCompatible;
            return false;
        }

        if (!reader.TryGetInt(out int challengeId))
        {
            rejectForce = true;
            response = PreAuthResponse.InvalidChallengeId;
            return false;
        }

        // If challengeId is not 0 then client sent challenge response.
        if (challengeId != 0)
        {
            if (!reader.TryGetBytesWithLength(out byte[] challengeResponse))
            {
                rejectForce = true;
                response = PreAuthResponse.InvalidChallengeResponse;
                return false;
            }
        }

        if (!reader.TryGetString(out string userId))
        {
            rejectForce = true;
            response = PreAuthResponse.InvalidUserId;
            return false;
        }

        if (string.IsNullOrEmpty(userId))
        {
            rejectForce = true;
            response = PreAuthResponse.UserIdIsEmpty;
            return false;
        }

        if (!reader.TryGetLong(out long expiration))
        {
            rejectForce = true;
            response = PreAuthResponse.InvalidExpiration;
            return false;
        }

        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiration)
        {
            rejectForce = true;
            response = PreAuthResponse.PreAuthExpired;
            return false;
        }

        if (!reader.TryGetByte(out byte rawCentralFlags))
        {
            rejectForce = true;
            response = PreAuthResponse.InvalidCentralFlags;
            return false;
        }

        if (!Enum.IsDefined(typeof(CentralAuthPreauthFlags), rawCentralFlags))
        {
            rejectForce = true;
            response = PreAuthResponse.CentralFlagsOutOfRange;
            return false;
        }

        CentralAuthPreauthFlags centralFlags = (CentralAuthPreauthFlags) rawCentralFlags;

        if (!reader.TryGetString(out string region))
        {
            rejectForce = true;
            response = PreAuthResponse.InvalidRegion;
            return false;
        }

        if (!reader.TryGetBytesWithLength(out byte[] signature))
        {
            rejectForce = true;
            response = PreAuthResponse.InvalidSignature;
            return false;
        }

        if (!ECDSA.VerifyBytes($"{userId};{rawCentralFlags};{region};{expiration}", signature, PublicKeyService.Key))
        {
            rejectForce = true;
            response = PreAuthResponse.BadSignature;
            return false;
        }

        preAuth = new PreAuth(clientVersion, backwardCompatibility, backwardRevision, userId, expiration, centralFlags, region, signature, connectionIp);
        response = PreAuthResponse.Valid;
        return true;
    }
}
