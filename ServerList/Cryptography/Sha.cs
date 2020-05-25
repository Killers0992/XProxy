using System;
using System.Security.Cryptography;
using System.Text;

namespace XProxy.ServerList.Cryptography
{
	public class Sha
	{
		public static byte[] Sha256(byte[] message)
		{
			byte[] result;
			using (SHA256 sha = SHA256.Create())
			{
				result = sha.ComputeHash(message);
			}
			return result;
		}

		public static byte[] Sha256(string message)
		{
			return Sha.Sha256(Utf8.GetBytes(message));
		}

		public static byte[] Sha256Hmac(byte[] key, byte[] message)
		{
			byte[] result;
			using (HMACSHA256 hmacsha = new HMACSHA256(key))
			{
				result = hmacsha.ComputeHash(message);
			}
			return result;
		}

		public static byte[] Sha512(string message)
		{
			return Sha.Sha512(Utf8.GetBytes(message));
		}

		public static byte[] Sha512(byte[] message)
		{
			byte[] result;
			using (SHA512 sha = SHA512.Create())
			{
				result = sha.ComputeHash(message);
			}
			return result;
		}

		public static byte[] Sha512Hmac(byte[] key, byte[] message)
		{
			byte[] result;
			using (HMACSHA512 hmacsha = new HMACSHA512(key))
			{
				result = hmacsha.ComputeHash(message);
			}
			return result;
		}

		public static string HashToString(byte[] hash)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (byte b in hash)
			{
				stringBuilder.Append(b.ToString("X2"));
			}
			return stringBuilder.ToString();
		}
	}
}
