using System;
using System.Collections.Generic;
using System.Text;
using Utf8Json;

namespace netproxy
{
	public readonly struct AuthenticatorPlayerObject : IEquatable<AuthenticatorPlayerObject>, IJsonSerializable
	{
		[SerializationConstructor]
		public AuthenticatorPlayerObject(string Id, string Ip, string RequestIp, string Asn, string AuthSerial, string VacSession)
		{
			this.Id = Id;
			this.Ip = Ip;
			this.RequestIp = RequestIp;
			this.Asn = Asn;
			this.AuthSerial = AuthSerial;
			this.VacSession = VacSession;
		}

		public bool Equals(AuthenticatorPlayerObject other)
		{
			return this.Id == other.Id && this.Ip == other.Ip && this.RequestIp == other.RequestIp && this.Asn == other.Asn && this.AuthSerial == other.AuthSerial && this.VacSession == other.VacSession;
		}

		public override bool Equals(object obj)
		{
			if (obj is AuthenticatorPlayerObject)
			{
				AuthenticatorPlayerObject other = (AuthenticatorPlayerObject)obj;
				return this.Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((((((this.Id != null) ? this.Id.GetHashCode() : 0) * 397 ^ ((this.Ip != null) ? this.Ip.GetHashCode() : 0)) * 397 ^ ((this.RequestIp != null) ? this.RequestIp.GetHashCode() : 0)) * 397 ^ ((this.Asn != null) ? this.Asn.GetHashCode() : 0)) * 397 ^ ((this.AuthSerial != null) ? this.AuthSerial.GetHashCode() : 0)) * 397 ^ ((this.VacSession != null) ? this.VacSession.GetHashCode() : 0);
		}

		public static bool operator ==(AuthenticatorPlayerObject left, AuthenticatorPlayerObject right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(AuthenticatorPlayerObject left, AuthenticatorPlayerObject right)
		{
			return !left.Equals(right);
		}

		public readonly string Id;
		public readonly string Ip;
		public readonly string RequestIp;
		public readonly string Asn;
		public readonly string AuthSerial;
		public readonly string VacSession;
	}
}
