using System.Text;

namespace netproxy
{
	public class Utf8
	{
		public static byte[] GetBytes(string data)
		{
			return Encoding.UTF8.GetBytes(data);
		}

		public static string GetString(byte[] data)
		{
			return Encoding.UTF8.GetString(data);
		}
	}
}