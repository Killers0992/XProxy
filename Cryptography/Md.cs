using System.Buffers;
using System.Security.Cryptography;
using System.Text;

namespace XProxy.Cryptography
{
	public static class Md
	{
		public static byte[] Md5(byte[] message)
		{
			using (var sha1 = MD5.Create())
			{
				return sha1.ComputeHash(message);
			}
		}

		public static byte[] Md5(byte[] message, int offset, int length)
		{
			using (var sha1 = MD5.Create())
			{
				return sha1.ComputeHash(message, offset, length);
			}
		}

		public static byte[] Md5(string message)
		{
			byte[] buffer = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(message.Length));
			int length = Utf8.GetBytes(message, buffer);
			byte[] result = Md5(buffer, 0, length);
			ArrayPool<byte>.Shared.Return(buffer);
			return result;
		}
	}
}
