using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

public static class Misc
{
	public static string LeadingZeroes(int integer, uint len, bool plusSign = false)
	{
		bool flag = integer < 0;
		if (flag)
		{
			integer *= -1;
		}
		string text = integer.ToString();
		while ((long)text.Length < (long)((ulong)len))
		{
			text = "0" + text;
		}
		return (flag ? "-" : (plusSign ? "+" : "")) + text;
	}

	public static string RemoveSpecialCharacters(string str)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (char c in str)
		{
			if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == ' ' || c == '-' || c == '.' || c == ',' || c == '_')
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	public static string StripTag(string input, string tag)
	{
		return Regex.Replace(input, "<.*?" + tag + ".*?>", string.Empty);
	}

	public static string StripTags(string input)
	{
		return Regex.Replace(input, "<.*?>", string.Empty);
	}

	public static int LevenshteinDistance(string s, string t)
	{
		int length = s.Length;
		int length2 = t.Length;
		int[,] array = new int[length + 1, length2 + 1];
		if (length == 0)
		{
			return length2;
		}
		if (length2 == 0)
		{
			return length;
		}
		int i = 0;
		while (i <= length)
		{
			array[i, 0] = i++;
		}
		int j = 0;
		while (j <= length2)
		{
			array[0, j] = j++;
		}
		for (int k = 1; k <= length; k++)
		{
			for (int l = 1; l <= length2; l++)
			{
				int num = (t[l - 1] == s[k - 1]) ? 0 : 1;
				array[k, l] = Math.Min(Math.Min(array[k - 1, l] + 1, array[k, l - 1] + 1), array[k - 1, l - 1] + num);
			}
		}
		return array[length, length2];
	}

	public static string LongestCommonSubstring(string a, string b)
	{
		int[,] array = new int[a.Length, b.Length];
		int num = 0;
		string result = "";
		for (int i = 0; i < a.Length; i++)
		{
			for (int j = 0; j < b.Length; j++)
			{
				if (a[i] == b[j])
				{
					array[i, j] = ((i == 0 || j == 0) ? 1 : (array[i - 1, j - 1] + 1));
					if (array[i, j] > num)
					{
						num = array[i, j];
						result = a.Substring(i - num + 1, num);
					}
				}
				else
				{
					array[i, j] = 0;
				}
			}
		}
		return result;
	}

	public static string LongestCommonSubstringOfAInB(string a, string b)
	{
		if (b.Length < a.Length)
		{
			string text = a;
			a = b;
			b = text;
		}
		for (int i = a.Length; i > 0; i--)
		{
			for (int j = a.Length - i; j <= a.Length - i; j++)
			{
				string text2 = a.Substring(j, i);
				if (b.Contains(text2))
				{
					return text2;
				}
			}
		}
		return "";
	}

	public static string Base64Encode(string plainText)
	{
		return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
	}

	public static string Base64Decode(string base64EncodedData)
	{
		byte[] bytes = Convert.FromBase64String(base64EncodedData);
		return Encoding.UTF8.GetString(bytes);
	}

	public static string ValidateIp(string text)
	{
		return new Regex("[^a-zA-Z0-9\\.\\:\\[\\]]").Replace(text, "");
	}

	public static bool ValidatePastebin(string text)
	{
		return new Regex("^[a-zA-Z0-9]{8}$").IsMatch(text);
	}

	public static string GetRuntimeVersion()
	{
		string result;
		try
		{
			result = RuntimeInformation.FrameworkDescription;
		}
		catch
		{
			result = "Not supported!";
		}
		return result;
	}

	
	public static bool CultureInfoTryParse(string name, out CultureInfo info)
	{
		bool result;
		try
		{
			info = CultureInfo.GetCultureInfo(name);
			result = true;
		}
		catch
		{
			info = null;
			result = false;
		}
		return result;
	}


	public static bool Contains(this string s, string value, StringComparison comparison)
	{
		return s.IndexOf(value, comparison) >= 0;
	}

	/*public static bool ParseVersion(out byte major, out byte minor)
	{
		bool result;
		try
		{
			string[] array = CustomNetworkManager.CompatibleVersions[0].Split(new char[]
			{
				'.'
			});
			if (array.Length > 1 && byte.TryParse(array[0], out major) && byte.TryParse(array[1], out minor))
			{
				result = true;
			}
			else
			{
				major = 0;
				minor = 0;
				result = false;
			}
		}
		catch
		{
			major = 0;
			minor = 0;
			result = false;
		}
		return result;
	}*/
}