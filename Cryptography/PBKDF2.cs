using System;
using System.Security.Cryptography;

namespace XProxy.Cryptography
{
	public static class PBKDF2
	{
		public static string Pbkdf2HashString(string password, byte[] salt, int iterations, int outputBytes)
		{
			return Convert.ToBase64String(Pbkdf2HashBytes(password, salt, iterations, outputBytes));
		}

		public static byte[] Pbkdf2HashBytes(string password, byte[] salt, int iterations, int outputBytes)
		{
			// Disposed un-disposed Rfc2898DeriveBytes @ Dankrushen
			using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt) {IterationCount = iterations})
			{
				return pbkdf2.GetBytes(outputBytes);
			}
		}
	}
}
