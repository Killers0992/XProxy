using System.Text;

public static class Utf8
{
	private static readonly UTF8Encoding _encoding = new UTF8Encoding(false);

	public static byte[] GetBytes(string data)
	{
		return _encoding.GetBytes(data);
	}

	public static int GetBytes(string data, byte[] buffer)
	{
		return _encoding.GetBytes(data, 0, data.Length, buffer, 0);
	}

	public static int GetBytes(string data, byte[] buffer, int offset)
	{
		return _encoding.GetBytes(data, 0, data.Length, buffer, offset);
	}

	public static string GetString(byte[] data)
	{
		return _encoding.GetString(data);
	}

	public static string GetString(byte[] data, int offset, int count)
	{
		return _encoding.GetString(data, offset, count);
	}
}
