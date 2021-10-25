using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using XProxy.ServerList.Cryptography;

namespace XProxy.ServerList
{
	public static class CentralServerKeyCache
	{
		public static string ReadCache()
		{
			string result;
			try
			{
				if (!File.Exists("./centralcache.txt"))
				{
					Console.WriteLine("Central server public key not found in cache.");
					result = null;
				}
				else if (!File.Exists("./centralkeysignature.txt"))
				{
					Console.WriteLine("Central server public key signature not found in cache.");
					result = null;
				}
				else
				{
					string[] source = File.ReadAllLines("./centralcache.txt");
					string[] array = File.ReadAllLines("./centralkeysignature.txt");
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
					if (File.Exists("./centralcache.txt"))
					{
						if (key == CentralServerKeyCache.ReadCache())
						{
							Console.WriteLine("Key cache is up to date.");
							return;
						}
						File.Delete("./centralcache.txt");
					}
					Console.WriteLine("Updating key cache...");
					File.WriteAllText($"./centralcache.txt", key, Encoding.UTF8);
					File.WriteAllText($"./centralkeysignature.txt", signature, Encoding.UTF8);
					Console.WriteLine("Key cache updated.");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Can't write public key cache - " + ex.Message);
			}
		}

		internal static readonly AsymmetricKeyParameter MasterKey = Cryptography.ECDSA.PublicKeyFromString("-----BEGIN PUBLIC KEY-----\r\nMIGbMBAGByqGSM49AgEGBSuBBAAjA4GGAAQAbL0YvrhVB2meqCq5XzjAJD8Ii0hb\r\nBHdIQ587N583cP8twjDhcITjZhBHJPJDuA85XdpgG04HwT0SD3WcAvoQXBUAUsG1\r\nLS9TR4urHwfgfroq4tH2HAQE6ZxFZeIFSglLO8nxySim4yKBj96HLG624lzKvzoD\r\nId+GOwjcd3XskOq9Dwc=\r\n-----END PUBLIC KEY-----");

		private const string MasterPublicKey = "-----BEGIN PUBLIC KEY-----\r\nMIGbMBAGByqGSM49AgEGBSuBBAAjA4GGAAQAbL0YvrhVB2meqCq5XzjAJD8Ii0hb\r\nBHdIQ587N583cP8twjDhcITjZhBHJPJDuA85XdpgG04HwT0SD3WcAvoQXBUAUsG1\r\nLS9TR4urHwfgfroq4tH2HAQE6ZxFZeIFSglLO8nxySim4yKBj96HLG624lzKvzoD\r\nId+GOwjcd3XskOq9Dwc=\r\n-----END PUBLIC KEY-----";
	}
}