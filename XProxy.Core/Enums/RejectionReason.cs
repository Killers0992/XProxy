using System;
using System.Collections.Generic;
using System.Text;

namespace XProxy.Enums
{
	public enum RejectionReason : byte
	{
		NotSpecified,
		ServerFull,
		InvalidToken,
		VersionMismatch,
		Error,
		AuthenticationRequired,
		Banned,
		NotWhitelisted,
		GloballyBanned,
		Geoblocked,
		Custom,
		ExpiredAuth,
		RateLimit,
		Challenge,
		InvalidChallengeKey,
		InvalidChallenge,
		Redirect,
		Delay,
		VerificationAccepted,
		VerificationRejected,
		CentralServerAuthRejected
	}
}
