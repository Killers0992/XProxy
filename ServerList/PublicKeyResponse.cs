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

		public override bool Equals(object obj)
		{
			if (obj is PublicKeyResponse)
			{
				PublicKeyResponse other = (PublicKeyResponse)obj;
				return this.Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((this.key != null) ? this.key.GetHashCode() : 0) * 397 ^ ((this.signature != null) ? this.signature.GetHashCode() : 0) ^ ((this.credits != null) ? this.credits.GetHashCode() : 0);
		}

		public static bool operator ==(PublicKeyResponse left, PublicKeyResponse right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(PublicKeyResponse left, PublicKeyResponse right)
		{
			return !left.Equals(right);
		}

		public readonly string key;
		public readonly string signature;
		public readonly string credits;
	}
}