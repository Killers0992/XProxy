using System;
using Utf8Json;

namespace XProxy.ServerList
{
	public readonly struct AuthenticatiorAuthReject : IEquatable<AuthenticatiorAuthReject>, IJsonSerializable
	{
		[SerializationConstructor]
		public AuthenticatiorAuthReject(string id, string reason)
		{
			this.Id = id;
			this.Reason = reason;
		}

		public bool Equals(AuthenticatiorAuthReject other)
		{
			return string.Equals(this.Id, other.Id) && string.Equals(this.Reason, other.Reason);
		}

		public override bool Equals(object obj)
		{
			if (obj is AuthenticatiorAuthReject)
			{
				AuthenticatiorAuthReject other = (AuthenticatiorAuthReject)obj;
				return this.Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((this.Id != null) ? this.Id.GetHashCode() : 0) * 397 ^ ((this.Reason != null) ? this.Reason.GetHashCode() : 0);
		}

		public static bool operator ==(AuthenticatiorAuthReject left, AuthenticatiorAuthReject right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(AuthenticatiorAuthReject left, AuthenticatiorAuthReject right)
		{
			return !left.Equals(right);
		}

		public readonly string Id;
		public readonly string Reason;
	}
}
