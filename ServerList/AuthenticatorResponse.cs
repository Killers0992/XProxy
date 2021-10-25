using System;
using Utf8Json;

namespace XProxy.ServerList
{
	public readonly struct AuthenticatorResponse : IEquatable<AuthenticatorResponse>, IJsonSerializable
	{
		[SerializationConstructor]
		public AuthenticatorResponse(bool success, bool verified, string error, string token, string[] messages, string[] actions, string[] authAccepted, AuthenticatiorAuthReject[] authRejected, string verificationChallenge, string verificationResponse)
		{
			this.success = success;
			this.verified = verified;
			this.error = error;
			this.token = token;
			this.messages = messages;
			this.actions = actions;
			this.authAccepted = authAccepted;
			this.authRejected = authRejected;
			this.verificationChallenge = verificationChallenge;
			this.verificationResponse = verificationResponse;
		}

		public bool Equals(AuthenticatorResponse other)
		{
			return this.success == other.success && this.verified == other.verified && string.Equals(this.error, other.error) && string.Equals(this.token, other.token) && this.messages == other.messages && this.actions == other.actions && this.authAccepted == other.authAccepted && this.authRejected == other.authRejected && this.verificationChallenge == other.verificationChallenge && this.verificationResponse == other.verificationResponse;
		}

		public readonly bool success;
		public readonly bool verified;
		public readonly string error;
		public readonly string token;
		public readonly string[] messages;
		public readonly string[] actions;
		public readonly string[] authAccepted;
		public readonly AuthenticatiorAuthReject[] authRejected;
		public readonly string verificationChallenge;
		public readonly string verificationResponse;
	}

}
