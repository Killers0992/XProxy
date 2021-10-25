using System;
using System.Collections.Generic;
using System.Text;
using Utf8Json;

namespace XProxy.ServerList
{
	public readonly struct PublicKeyResponse : IEquatable<PublicKeyResponse>, IJsonSerializable
	{
		[SerializationConstructor]
		public PublicKeyResponse(string key, string signature, string credits)
		{
			this.key = key;
			this.signature = signature;
			this.credits = credits;
		}

		public bool Equals(PublicKeyResponse other)
		{
			return this.key == other.key && this.signature == other.signature && this.credits == other.credits;
		}

		public readonly string key;
		public readonly string signature;
		public readonly string credits;
	}
}