using System;
using Utf8Json;

namespace XProxy.ServerList
{
	public readonly struct AuthenticatorResponse : IEquatable<AuthenticatorResponse>, IJsonSerializable
	{
		[SerializationConstructor]
		public AuthenticatorResponse(bool success, bool verified, string error, string token, string[] messages, string[] actions, string[] authAccepted, AuthenticatiorAuthReject[] authRejected)
		{
			this.success = success;
			this.verified = verified;
			this.error = error;
			this.token = token;
			this.messages = messages;
			this.actions = actions;
			this.authAccepted = authAccepted;
			this.authRejected = authRejected;
		}

		public bool Equals(AuthenticatorResponse other)
		{
			return this.success == other.success && this.verified == other.verified && string.Equals(this.error, other.error) && string.Equals(this.token, other.token) && this.messages == other.messages && this.actions == other.actions && this.authAccepted == other.authAccepted && this.authRejected == other.authRejected;
		}

		public override bool Equals(object obj)
		{
			if (obj is AuthenticatorResponse)
			{
				AuthenticatorResponse other = (AuthenticatorResponse)obj;
				return this.Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((((((this.success.GetHashCode() * 397 ^ this.verified.GetHashCode()) * 397 ^ ((this.error != null) ? this.error.GetHashCode() : 0)) * 397 ^ ((this.token != null) ? this.token.GetHashCode() : 0)) * 397 ^ ((this.messages != null) ? this.messages.GetHashCode() : 0)) * 397 ^ ((this.actions != null) ? this.actions.GetHashCode() : 0)) * 397 ^ ((this.authAccepted != null) ? this.authAccepted.GetHashCode() : 0)) * 397 ^ ((this.authRejected != null) ? this.authRejected.GetHashCode() : 0);
		}

		public static bool operator ==(AuthenticatorResponse left, AuthenticatorResponse right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(AuthenticatorResponse left, AuthenticatorResponse right)
		{
			return !left.Equals(right);
		}

		public readonly bool success;
		public readonly bool verified;
		public readonly string error;
		public readonly string token;
		public readonly string[] messages;
		public readonly string[] actions;
		public readonly string[] authAccepted;
		public readonly AuthenticatiorAuthReject[] authRejected;
	}
}
