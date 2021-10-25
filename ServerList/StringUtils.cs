using System;
using System.Buffers;
using System.Text;

namespace XProxy.ServerList
{
    public static class StringUtils
    {
		public static string Base64Encode(string plainText)
		{
			byte[] array = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(plainText.Length));
			int bytes = Encoding.UTF8.GetBytes(plainText, 0, plainText.Length, array, 0);
			string result = Convert.ToBase64String(array, 0, bytes);
			ArrayPool<byte>.Shared.Return(array, false);
			return result;
		}

		public static string Base64Decode(string base64EncodedData)
		{
			byte[] bytes = Convert.FromBase64String(base64EncodedData);
			return Encoding.UTF8.GetString(bytes);
		}
	}
}
