using System;

namespace XProxy.Enums
{
	[Flags]
	public enum CentralAuthPreauthFlags : byte
	{
		None = 0,
		ReservedSlot = 1,
		IgnoreBans = 2,
		IgnoreWhitelist = 4,
		IgnoreGeoblock = 8,
		GloballyBanned = 16,
		NorthwoodStaff = 32,
		AuthRejected = 64
	}
}
