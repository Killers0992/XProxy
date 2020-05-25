using netproxy;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public static class CentralServerKeyCache
{
	public static string ReadCache()
	{
		string result;
		try
		{
			string appFolder = "/home/sl/.config/SCP Secret Laboratory/internal/";
			string path = appFolder + "internal/KeyCache";
			string path2 = appFolder + "internal/KeySignatureCache";
			if (!File.Exists(path))
			{
				Console.WriteLine("Central server public key not found in cache.");
				result = null;
			}
			else if (!File.Exists(path2))
			{
				Console.WriteLine("Central server public key signature not found in cache.");
				result = null;
			}
			else
			{
				string[] source = FileManager.ReadAllLines(path);
				string[] array = FileManager.ReadAllLines(path2);
				if (array.Length == 0)
				{
					Console.WriteLine("Can't load central server public key from cache - empty signature.");
					result = null;
				}
				else
				{
					string text = source.Aggregate("", (string current, string line) => current + line + "\r\n").Trim();
					try
					{
						if (ECDSA.Verify(text, array[0], CentralServerKeyCache.MasterKey))
						{
							result = text;
						}
						else
						{
							Console.WriteLine("Invalid signature of Central Server Key in cache!");
							result = null;
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine("Can't load central server public key from cache - " + ex.Message);
						result = null;
					}
				}
			}
		}
		catch (Exception ex2)
		{
			Console.WriteLine("Can't read public key cache - " + ex2.Message);
			result = null;
		}
		return result;
	}

	public static void SaveCache(string key, string signature)
	{
		try
		{
			if (!ECDSA.Verify(key, signature, CentralServerKeyCache.MasterKey))
			{
				Console.WriteLine("Invalid signature of Central Server Key!");
			}
			else
			{
				string appFolder = "/home/sl/.config/SCP Secret Laboratory/";
				string path = appFolder + "internal/KeyCache";
				if (!Directory.Exists("/home/sl/.config/SCP Secret Laboratory/internal/"))
				{
					Directory.CreateDirectory("/home/sl/.config/SCP Secret Laboratory/internal/");
				}
				if (File.Exists(path))
				{
					if (key == CentralServerKeyCache.ReadCache())
					{
						Console.WriteLine("Key cache is up to date.");
						return;
					}
					File.Delete(path);
				}
				Console.WriteLine("Updating key cache...");
				FileManager.WriteStringToFile(key, path);
				FileManager.WriteStringToFile(signature, appFolder + "internal/KeySignatureCache");
				Console.WriteLine("Key cache updated.");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine("Can't write public key cache - " + ex.Message);
		}
	}

	public const string CacheLocation = "internal/KeyCache";
	public const string CacheSignatureLocation = "internal/KeySignatureCache";
	public const string InternalDir = "internal/";
	public static readonly AsymmetricKeyParameter MasterKey = ECDSA.PublicKeyFromString("-----BEGIN PUBLIC KEY-----\r\nMIGbMBAGByqGSM49AgEGBSuBBAAjA4GGAAQAmxZRMP03JfPEP/qt7n34Ryi74CDe\r\nRZy4er5dQynKaQ3vl1F4VRsSGN+jBrZPcX3GB2u0OTXNUA8hcIDRhVb+GgYAcDmY\r\n+7utHYAZBK3APSxGn46p1+IAChsgl9r93bQz7AJVxxWHYKEA78jMVz6qKHlqKc6a\r\nkUswVSYosQGvw/Agzb0=\r\n-----END PUBLIC KEY-----");
	public const string MasterPublicKey = "-----BEGIN PUBLIC KEY-----\r\nMIGbMBAGByqGSM49AgEGBSuBBAAjA4GGAAQAmxZRMP03JfPEP/qt7n34Ryi74CDe\r\nRZy4er5dQynKaQ3vl1F4VRsSGN+jBrZPcX3GB2u0OTXNUA8hcIDRhVb+GgYAcDmY\r\n+7utHYAZBK3APSxGn46p1+IAChsgl9r93bQz7AJVxxWHYKEA78jMVz6qKHlqKc6a\r\nkUswVSYosQGvw/Agzb0=\r\n-----END PUBLIC KEY-----";
}