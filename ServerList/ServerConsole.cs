using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using Utf8Json;
using XProxy.ServerList.Authenticator;
using XProxy.ServerList.Cryptography;

namespace XProxy.ServerList
{
    public class ServerConsole
    {
		public static bool _disposing;
		public static string Password;
		public static bool Update;
		public static string _verificationPlayersList = string.Empty;
		public static bool ScheduleTokenRefresh;
		public static string Ip;
		public int Port;
		public static string VerKey;
		public static bool _emailSet;
		public static bool _printedNotVerifiedMessage;
		public static Thread _verificationRequestThread;
		public static Thread _refreshPublicKeyOnceThread;
		public static Thread _refreshPublicKeyThread;
		public static AsymmetricKeyParameter PublicKey;
		public static HttpQueryMode HttpMode;
		public static bool LockHttpMode;
		public int PlayersOnline;

		public ServerConsole(int serverPort)
        {
			Port = serverPort;
			PlayersOnline = 0;
			string ip = new WebClient().DownloadString("https://api.scpslgame.com/ip.php");
			ServerConsole.Ip = (ip.EndsWith(".") ? ip.Remove(ip.Length - 1) : ip);
			RunRefreshPublicKey();
			RunServer();
		}

		public static void RefreshEmailSetStatus()
		{
			_emailSet = true;
		}
		public void RunServer()
		{
			if (ServerConsole._verificationRequestThread != null && ServerConsole._verificationRequestThread.IsAlive)
			{
				ServerConsole._verificationRequestThread.Abort();
			}
			ServerConsole._verificationRequestThread = new Thread(new ThreadStart(this.RefreshServerData))
			{
				IsBackground = true,
				Priority = System.Threading.ThreadPriority.AboveNormal,
				Name = "SCP:SL Server list thread"
			};
			ServerConsole._verificationRequestThread.Start();
		}

		public static void RunRefreshPublicKey()
		{
			if (ServerConsole._refreshPublicKeyThread != null && ServerConsole._refreshPublicKeyThread.IsAlive)
			{
				ServerConsole._refreshPublicKeyThread.Abort();
			}
			ServerConsole._refreshPublicKeyThread = new Thread(new ThreadStart(ServerConsole.RefreshPublicKey))
			{
				IsBackground = true,
				Priority = System.Threading.ThreadPriority.Normal,
				Name = "SCP:SL Public key refreshing"
			};
			ServerConsole._refreshPublicKeyThread.Start();
		}

		public static void RunRefreshPublicKeyOnce()
		{
			if (ServerConsole._refreshPublicKeyOnceThread != null && ServerConsole._refreshPublicKeyOnceThread.IsAlive)
			{
				ServerConsole._refreshPublicKeyOnceThread.Abort();
			}
			ServerConsole._refreshPublicKeyOnceThread = new Thread(new ThreadStart(ServerConsole.RefreshPublicKeyOnce))
			{
				IsBackground = true,
				Priority = System.Threading.ThreadPriority.AboveNormal,
				Name = "SCP:SL Public key refreshing ON DEMAND"
			};
			ServerConsole._refreshPublicKeyOnceThread.Start();
		}

		public static void RefreshPublicKey()
		{
			string text = CentralServerKeyCache.ReadCache();
			string text2 = string.Empty;
			string b = string.Empty;
			if (!string.IsNullOrEmpty(text))
			{
				ServerConsole.PublicKey = ECDSA.PublicKeyFromString(text);
				text2 = Sha.HashToString(Sha.Sha256(ECDSA.KeyToString(ServerConsole.PublicKey)));
				Console.WriteLine("Loaded central server public key from cache.\nSHA256 of public key: " + text2);
			}
			Console.WriteLine("Downloading public key from central server...");
			while (!ServerConsole._disposing)
			{
				try
				{
					PublicKeyResponse publicKeyResponse = JsonConvert.DeserializeObject<PublicKeyResponse>(HttpQuery.Get(string.Format("{0}v4/publickey.php?major={1}", "https://api.scpslgame.com/", 11)));
					if (!ECDSA.Verify(publicKeyResponse.key, publicKeyResponse.signature, CentralServerKeyCache.MasterKey))
					{
						Console.WriteLine("Can't refresh central server public key - invalid signature!");
						Thread.Sleep(360000);
						continue;
					}
					ServerConsole.PublicKey = ECDSA.PublicKeyFromString(publicKeyResponse.key);
					string text3 = Sha.HashToString(Sha.Sha256(ECDSA.KeyToString(ServerConsole.PublicKey)));
					if (text3 != b)
					{
						b = text3;
						Console.WriteLine("Downloaded public key from central server.\nSHA256 of public key: " + text3);
						if (text3 != text2)
						{
							CentralServerKeyCache.SaveCache(publicKeyResponse.key, publicKeyResponse.signature);
						}
						else
						{
							Console.WriteLine("SHA256 of cached key matches, no need to update cache.");
						}
					}
					else
					{
						Console.WriteLine("Refreshed public key of central server - key hash not changed.");
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("Can't refresh central server public key - " + ex.Message);
				}
				Thread.Sleep(360000);
			}
		}

		public static void RefreshPublicKeyOnce()
		{
			try
			{
				PublicKeyResponse publicKeyResponse = JsonConvert.DeserializeObject<PublicKeyResponse>(HttpQuery.Get("https://api.scpslgame.com/v4/publickey.php"));
				if (!ECDSA.Verify(publicKeyResponse.key, publicKeyResponse.signature, CentralServerKeyCache.MasterKey))
				{
					Console.WriteLine("Can't refresh central server public key - invalid signature!");
				}
				else
				{
					ServerConsole.PublicKey = ECDSA.PublicKeyFromString(publicKeyResponse.key);
					string str = Sha.HashToString(Sha.Sha256(ECDSA.KeyToString(ServerConsole.PublicKey)));
					Console.WriteLine("Downloaded public key from central server.\nSHA256 of public key: " + str);
					CentralServerKeyCache.SaveCache(publicKeyResponse.key, publicKeyResponse.signature);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Can't refresh central server public key - " + ex.Message);
			}
		}

		public void RefreshServerData()
		{
			bool flag = true;
			byte b = 0;
			RefreshEmailSetStatus();
			RefreshToken(true);
			while (!_disposing)
			{
				b += 1;
				if (!flag && string.IsNullOrEmpty(Password) && b < 15)
				{
					if (b == 5 || b == 12 || ScheduleTokenRefresh)
					{
						RefreshToken(false);
					}
				}
				else
				{
					flag = false;
					ServerConsole.Update = (ServerConsole.Update || b == 10);
					string str = string.Empty;
					try
					{
						List<AuthenticatorPlayerObject> list = new List<AuthenticatorPlayerObject>();
						/*while (!ServerConsole.NewPlayers.IsEmpty)
						{
							num++;
							if (num > count + 30)
							{
								break;
							}
						}*/
						str = JsonConvert.SerializeObject(new AuthenticatorPlayerObjects(list.ToArray()));
					}
					catch (Exception ex2)
					{
						Console.WriteLine("[VERIFICATION THREAD] Exception in New Players processing: " + ex2.Message);
						Console.WriteLine(ex2.StackTrace);
					}
					List<string> list2 = ServerConsole.Update ? new List<string>
					{
						"ip=" + ServerConsole.Ip,
						string.Concat(new object[]
						{
							"players=",
							PlayersOnline,
							"/",
							50
						}),
						"playersList=" + _verificationPlayersList,
						"newPlayers=" + str,
						"port=" + Port,
						"pastebin=" + Program.config.ProxyPastebin,
						"gameVersion=" + Program.config.GameVersion,
						"version=2",
						"update=1",
						"info=" + Misc.Base64Encode(Program.config.ServerName).Replace('+', '-'),
						"privateBeta=" + Program.config.IsPrivateBeta,
						"staffRA=" + true,
						"friendlyFire=" + false,
						"geoblocking=" + (byte)0,
						"modded=" + true,
						"cgs=" + true,
						"whitelist=" + false,
						"accessRestriction=" + false,
						"emailSet=" + true,
						"enforceSameIp=" + true,
						"enforceSameAsn=" + true
					} : new List<string>
					{
						"ip=" + ServerConsole.Ip,
						string.Concat(new object[]
						{
							"players=",
							PlayersOnline,
							"/",
							50
						}),
						"newPlayers=" + str,
						"port=" + Port,
						"version=2",
						"enforceSameIp=" + true,
						"enforceSameAsn=" + true
					};
					if (!string.IsNullOrEmpty(Password))
					{
						list2.Add("passcode=" + Password);
					}
					ServerConsole.Update = false;
					if (!AuthenticatorQuery.SendData(list2) && !ServerConsole._printedNotVerifiedMessage)
					{
						ServerConsole._printedNotVerifiedMessage = true;
						Console.WriteLine("Your server won't be visible on the public server list - (" + ServerConsole.Ip + ")LOGTYPE-8");
						if (!_emailSet)
						{
							Console.WriteLine("If you are 100% sure that the server is working, can be accessed from the Internet and YOU WANT TO MAKE IT PUBLIC, please set up your email in configuration file (\"contact_email\" value) and restart the server. LOGTYPE-8");
						}
						else
						{
							Console.WriteLine("If you are 100% sure that the server is working, can be accessed from the Internet and YOU WANT TO MAKE IT PUBLIC please email following information: LOGTYPE-8");
							Console.WriteLine("- IP address of server (probably " + ServerConsole.Ip + ") LOGTYPE-8");
							Console.WriteLine("- is this static or dynamic IP address (most of home adresses are dynamic) LOGTYPE-8");
							Console.WriteLine("PLEASE READ rules for verified servers first: https://scpslgame.com/Verified_server_rules.pdf LOGTYPE-8");
							Console.WriteLine("send us that information to: server.verification@scpslgame.com (server.verification at scpslgame.com) LOGTYPE-8");
							Console.WriteLine("if you can't see the AT sign in console (in above line): server.verification AT scpslgame.com LOGTYPE-8");
							Console.WriteLine("email must be sent from email address set as \"contact_email\" in your config file (current value: ). LOGTYPE-8");
						}
					}
					else
					{
						ServerConsole._printedNotVerifiedMessage = true;
					}
				}
				if (b >= 15)
				{
					b = 0;
				}
				Thread.Sleep(5000);
				if (ScheduleTokenRefresh || b == 0)
				{
					RefreshToken(false);
				}
			}
		}

		public void RunCentralServerCommand(string cmd, string args)
		{
			cmd = cmd.ToLower();
			List<string> list = new List<string>
			{
				"ip=" + ServerConsole.Ip,
				"port=" + Port,
				"cmd=" + StringUtils.Base64Encode(cmd),
				"args=" + StringUtils.Base64Encode(args)
			};
			if (!string.IsNullOrEmpty(Password))
			{
				list.Add("passcode=" + Password);
			}
			try
			{
				string str = HttpQuery.Post("https://api.scpslgame.com/centralcommands/" + cmd + ".php", HttpQuery.ToPostArgs(list));
				Console.WriteLine("[" + cmd + "] " + str, ConsoleColor.Gray);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Could not execute the central server command \"" + cmd + "\" - (LOCAL EXCEPTION) " + ex.Message, ConsoleColor.Red);
			}
		}

		public void RefreshToken(bool init = false)
		{
			ScheduleTokenRefresh = false;

			if (!File.Exists("./verkey.txt"))
				File.WriteAllText("./verkey.txt", "none");

			if (VerKey == null)
				VerKey = File.ReadAllText("./verkey.txt");

			if (string.IsNullOrEmpty(VerKey))
			{
				return;
			}
			if (!init && string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(VerKey))
			{
				Console.WriteLine("Verification token loaded! Server probably will be listed on public list.");
			}
			if (Password != VerKey)
			{
				Console.WriteLine("Verification token reloaded.");
				ServerConsole.Update = true;
			}
			Password = VerKey;
		}
	}
}
