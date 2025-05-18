namespace XProxy.Enums;

public enum PreAuthResponse : byte
{
    Valid,
    InvalidClientType,
    ClientTypeOutOfRange,
    ForbiddenClientType,
    InvalidMajorVersion,
    InvalidMinorVersion,
    InvalidRevisionVersion,
    InvalidBackwardCompatibility,
    InvalidBackwardRevision,
    VersionNotCompatible,
    InvalidChallengeId,
    InvalidChallengeResponse,
    InvalidUserId,
    UserIdIsEmpty,
    InvalidExpiration,
    PreAuthExpired,
    InvalidCentralFlags,
    CentralFlagsOutOfRange,
    InvalidRegion,
    InvalidSignature,
    BadSignature
}
