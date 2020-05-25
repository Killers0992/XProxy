using System;
using System.Collections.Generic;
using System.Text;
using Utf8Json;

namespace XProxy.ServerList
{
	public readonly struct AuthenticatorPlayerObjects : IEquatable<AuthenticatorPlayerObjects>, IJsonSerializable
	{
		[SerializationConstructor]
		public AuthenticatorPlayerObjects(AuthenticatorPlayerObject[] objects)
		{
			this.objects = objects;
		}

		public bool Equals(AuthenticatorPlayerObjects other)
		{
			return this.objects == other.objects;
		}

		public override bool Equals(object obj)
		{
			if (obj is AuthenticatorPlayerObjects)
			{
				AuthenticatorPlayerObjects other = (AuthenticatorPlayerObjects)obj;
				return this.Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			if (this.objects == null)
			{
				return 0;
			}
			return this.objects.GetHashCode();
		}

		public static bool operator ==(AuthenticatorPlayerObjects left, AuthenticatorPlayerObjects right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(AuthenticatorPlayerObjects left, AuthenticatorPlayerObjects right)
		{
			return !left.Equals(right);
		}

		public readonly AuthenticatorPlayerObject[] objects;
	}
}
