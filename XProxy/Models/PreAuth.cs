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

    public static bool TryRead(BaseListener listener, string connectionIp, NetDataReader reader, ref DisconnectType response, ref bool rejectForce, ref PreAuth preAuth)
    {
        if (!reader.TryGetByte(out byte rawClientType))
        {
            rejectForce = true;
            response = DisconnectType.InvalidClientType;
            return false;
        }

        if (!Enum.IsDefined(typeof(ClientType), rawClientType))
        {
            rejectForce = true;
            response = DisconnectType.ClientTypeOutOfRange;
            return false;
        }

        ClientType clientType = (ClientType)rawClientType;

        if (clientType == ClientType.VerificationService)
        {
            rejectForce = true;
            response = DisconnectType.ForbiddenClientType;
            return false;
        }

        if (!reader.TryGetByte(out byte major))
        {
            rejectForce = true;
            response = DisconnectType.InvalidMajorVersion;
            return false;
        }

        if (!reader.TryGetByte(out byte minor))
        {
            rejectForce = true;
            response = DisconnectType.InvalidMinorVersion;
            return false;
        }

        if (!reader.TryGetByte(out byte revision))
        {
            rejectForce = true;
            response = DisconnectType.InvalidRevisionVersion;
            return false;
        }

        if (!reader.TryGetBool(out bool backwardCompatibility))
        {
            rejectForce = true;
            response = DisconnectType.InvalidBackwardCompatibility;
            return false;
        }

        byte backwardRevision = 0;

        if (backwardCompatibility)
        {
            if (!reader.TryGetByte(out backwardRevision))
            {
                rejectForce = true;
                response = DisconnectType.InvalidBackwardRevision;
                return false;
            }
        }

        Version clientVersion = new Version(major, minor, revision);

        if (!listener.GameVersion.ValidateGameVersion(clientVersion, backwardCompatibility, backwardRevision))
        {
            response = DisconnectType.VersionNotCompatible;
            return false;
        }

        if (!reader.TryGetInt(out int challengeId))
        {
            rejectForce = true;
            response = DisconnectType.InvalidChallengeId;
            return false;
        }

        // If challengeId is not 0 then client sent challenge response.
        if (challengeId != 0)
        {
            if (!reader.TryGetBytesWithLength(out byte[] challengeResponse))
            {
                rejectForce = true;
                response = DisconnectType.InvalidChallengeResponse;
                return false;
            }
        }

        if (!reader.TryGetString(out string userId))
        {
            rejectForce = true;
            response = DisconnectType.InvalidUserId;
            return false;
        }

        if (string.IsNullOrEmpty(userId))
        {
            rejectForce = true;
            response = DisconnectType.UserIdIsEmpty;
            return false;
        }

        if (!reader.TryGetLong(out long expiration))
        {
            rejectForce = true;
            response = DisconnectType.InvalidExpiration;
            return false;
        }

        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiration)
        {
            rejectForce = true;
            response = DisconnectType.PreAuthExpired;
            return false;
        }

        if (!reader.TryGetByte(out byte rawCentralFlags))
        {
            rejectForce = true;
            response = DisconnectType.InvalidCentralFlags;
            return false;
        }

        if (!Enum.IsDefined(typeof(CentralAuthPreauthFlags), rawCentralFlags))
        {
            rejectForce = true;
            response = DisconnectType.CentralFlagsOutOfRange;
            return false;
        }

        CentralAuthPreauthFlags centralFlags = (CentralAuthPreauthFlags) rawCentralFlags;

        if (!reader.TryGetString(out string region))
        {
            rejectForce = true;
            response = DisconnectType.InvalidRegion;
            return false;
        }

        if (!reader.TryGetBytesWithLength(out byte[] signature))
        {
            rejectForce = true;
            response = DisconnectType.InvalidSignature;
            return false;
        }

        if (!ECDSA.VerifyBytes($"{userId};{rawCentralFlags};{region};{expiration}", signature, PublicKeyService.Key))
        {
            rejectForce = true;
            response = DisconnectType.BadSignature;
            return false;
        }

        preAuth = new PreAuth(clientVersion, backwardCompatibility, backwardRevision, userId, expiration, centralFlags, region, signature, connectionIp);
        response = DisconnectType.Valid;
        return true;
    }
}
