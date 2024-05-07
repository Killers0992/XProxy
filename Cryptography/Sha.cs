using System.Buffers;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NorthwoodLib.Pools;

namespace XProxy.Cryptography
{
	public static class Sha
	{
		public static byte[] Sha1(byte[] message)
		{
			// Disposed un-disposed SHA1 @ Dankrushen
			using (var sha1 = SHA1.Create())
			{
				return sha1.ComputeHash(message);
			}
		}

		public static byte[] Sha1(byte[] message, int offset, int length)
		{
			// Disposed un-disposed SHA1 @ Dankrushen
			using (var sha1 = SHA1.Create())
			{
				return sha1.ComputeHash(message, offset, length);
			}
		}

		public static byte[] Sha1(string message)
		{
			byte[] buffer = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(message.Length));
			int length = Utf8.GetBytes(message, buffer);
			byte[] result = Sha1(buffer, 0, length);
			ArrayPool<byte>.Shared.Return(buffer);
			return result;
		}

		public static byte[] Sha256(byte[] message)
		{
			// Disposed un-disposed SHA256 @ Dankrushen
			using (var sha256 = SHA256.Create())
			{
				return sha256.ComputeHash(message);
			}
		}

		public static byte[] Sha256(byte[] message, int offset, int length)
		{
			// Disposed un-disposed SHA256 @ Dankrushen
			using (var sha256 = SHA256.Create())
			{
				return sha256.ComputeHash(message, offset, length);
			}
		}

		public static byte[] Sha256(string message)
		{
			byte[] buffer = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(message.Length));
			int length = Utf8.GetBytes(message, buffer);
			byte[] result = Sha256(buffer, 0, length);
			ArrayPool<byte>.Shared.Return(buffer);
			return result;
		}

#if !HEADLESS
		public static byte[] Sha256File(string path)
		{
			using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var sha256 = SHA256.Create())
				return sha256.ComputeHash(fs);
		}
#endif

		public static byte[] Sha256Hmac(byte[] key, byte[] message)
		{
			// Disposed un-disposed HMACSHA256 @ Dankrushen
			using (var hash = new HMACSHA256(key))
			{
				return hash.ComputeHash(message);
			}
		}

		public static byte[] Sha512(string message)
		{
			byte[] buffer = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(message.Length));
			int length = Utf8.GetBytes(message, buffer);
			byte[] result = Sha512(buffer, 0, length);
			ArrayPool<byte>.Shared.Return(buffer);
			return result;
		}

		public static byte[] Sha512(byte[] message)
		{
			// Disposed un-disposed SHA512 @ Dankrushen
			using (var sha512 = SHA512.Create())
			{
				return sha512.ComputeHash(message);
			}
		}

		public static byte[] Sha512(byte[] message, int offset, int length)
		{
			// Disposed un-disposed SHA512 @ Dankrushen
			using (var sha512 = SHA512.Create())
			{
				return sha512.ComputeHash(message, offset, length);
			}
		}

		public static byte[] Sha512Hmac(byte[] key, byte[] message)
		{
			// Disposed un-disposed HMACSHA512 @ Dankrushen
			using (var hash = new HMACSHA512(key))
			{
				return hash.ComputeHash(message);
			}
		}

		public static byte[] Sha512Hmac(byte[] key, int offset, int length, byte[] message)
		{
			// Disposed un-disposed HMACSHA512 @ Dankrushen
			using (var hash = new HMACSHA512(key))
			{
				return hash.ComputeHash(message, offset, length);
			}
		}

		public static string HashToString(byte[] hash)
		{
			var result = StringBuilderPool.Shared.Rent();
			foreach (var t in hash)
				result.Append(t.ToString("X2"));
			string text = result.ToString();
			StringBuilderPool.Shared.Return(result);

			return text;
		}
	}
}
