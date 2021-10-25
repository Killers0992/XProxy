using System;
using System.Collections.Generic;
using System.Text;
using Utf8Json;

namespace XProxy.ServerList
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

		public readonly string Id;
		public readonly string Ip;
		public readonly string RequestIp;
		public readonly string Asn;
		public readonly string AuthSerial;
		public readonly string VacSession;
	}
}
