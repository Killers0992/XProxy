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

		public readonly AuthenticatorPlayerObject[] objects;
	}
}
