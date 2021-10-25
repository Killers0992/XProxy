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

		public readonly string Id;
		public readonly string Reason;
	}
}
