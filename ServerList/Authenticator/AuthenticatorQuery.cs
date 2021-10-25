using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Utf8Json;

namespace XProxy.ServerList.Authenticator
{
	public static class AuthenticatorQuery
	{
		public static bool SendData(IEnumerable<string> data)
		{
			bool result;
			try
			{
				string text = HttpQuery.Post("https://api.scpslgame.com/v4/authenticator.php", HttpQuery.ToPostArgs(data));
				result = (text.StartsWith("{\"") ? AuthenticatorQuery.ProcessResponse(text) : AuthenticatorQuery.ProcessLegacyResponse(text));
			}
			catch (Exception ex)
			{
				Console.WriteLine("Could not update server data on server list - (LOCAL EXCEPTION) " + ex.Message + "LOGTYPE-4");
				result = false;
			}
			return result;
		}

		public static void SendContactAddress()
		{
			try
			{
				List<string> list = new List<string>
				{
					"ip=" + ServerConsole.Ip,
					"port=" + 7777,
					"version=2",
					"address=" + Misc.Base64Encode(Program.config.Email)
				};
				if (!string.IsNullOrEmpty(ServerConsole.Password))
				{
					list.Add("passcode=" + ServerConsole.Password);
				}
				HttpQuery.Post("https://api.scpslgame.com/v4/contactaddress.php", HttpQuery.ToPostArgs(list));
			}
			catch
			{
			}
		}

		private static bool ProcessResponse(string response)
		{
			bool result;
			try
			{
				AuthenticatorResponse authenticatorResponse = JsonConvert.DeserializeObject<AuthenticatorResponse>(response);
				if (!string.IsNullOrEmpty(authenticatorResponse.verificationChallenge) && !string.IsNullOrEmpty(authenticatorResponse.verificationResponse))
				{
					Console.WriteLine("Verification challenge and response have been obtained.", ConsoleColor.Green);
				}
				if (!authenticatorResponse.success)
				{
					Console.WriteLine("Could not update server data on server list - " + authenticatorResponse.error, ConsoleColor.DarkRed);
					result = false;
				}
				else
				{
					if (!string.IsNullOrEmpty(authenticatorResponse.token))
					{
						Console.WriteLine("Received verification token from central server.", ConsoleColor.Gray);
						AuthenticatorQuery.SaveNewToken(authenticatorResponse.token);
					}
					if (authenticatorResponse.actions != null && authenticatorResponse.actions.Length != 0)
					{
						string[] array = authenticatorResponse.actions;
						for (int i = 0; i < array.Length; i++)
						{
							AuthenticatorQuery.HandleAction(array[i]);
						}
					}
					if (authenticatorResponse.messages != null && authenticatorResponse.messages.Length != 0)
					{
						foreach (string str in authenticatorResponse.messages)
						{
							Console.WriteLine("[MESSAGE FROM CENTRAL SERVER] " + str, ConsoleColor.Cyan);
						}
					}
					if (authenticatorResponse.authAccepted != null && authenticatorResponse.authAccepted.Length != 0)
					{
						foreach (string str2 in authenticatorResponse.authAccepted)
						{
							Console.WriteLine("Authentication token of player " + str2 + " has been confirmed by central server.", ConsoleColor.Gray);
						}
					}
					if (authenticatorResponse.authRejected != null && authenticatorResponse.authRejected.Length != 0)
					{
					
					}
					result = authenticatorResponse.verified;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Could not update server data on server list - (LOCAL EXCEPTION) " + ex.Message, ConsoleColor.DarkRed);
				result = false;
			}
			return result;
		}

		public static bool ProcessLegacyResponse(string response)
		{
			if (response == "YES")
			{
				return true;
			}
			if (response.StartsWith("New code generated:"))
			{
				try
				{
					string text = response.Remove(0, response.IndexOf(":", StringComparison.Ordinal)).Remove(response.IndexOf(":", StringComparison.Ordinal));
					while (text.Contains(":"))
					{
						text = text.Replace(":", string.Empty);
					}
					ServerConsole.VerKey = text;
					File.WriteAllText("./config.json", JsonConvert.SerializeObject(Program.config, Formatting.Indented));
					Console.WriteLine("New password saved.LOGTYPE-3");
					ServerConsole.Update = true;
					return true;
				}
				catch
				{
					Console.WriteLine("New password could not be saved.LOGTYPE-4");
					return true;
				}
			}
			if (response.Contains(":Restart:"))
			{
				AuthenticatorQuery.HandleAction("Restart");
			}
			else if (response.Contains(":RoundRestart:"))
			{
				AuthenticatorQuery.HandleAction("RoundRestart");
			}
			else if (response.Contains(":UpdateData:"))
			{
				AuthenticatorQuery.HandleAction("UpdateData");
			}
			else if (response.Contains(":RefreshKey:"))
			{
				AuthenticatorQuery.HandleAction("RefreshKey");
			}
			else if (response.Contains(":Message - "))
			{
				string text2 = response.Substring(response.IndexOf(":Message - ", StringComparison.Ordinal) + 11);
				text2 = text2.Substring(0, text2.IndexOf(":::", StringComparison.Ordinal));
				Console.WriteLine("[MESSAGE FROM CENTRAL SERVER] " + text2 + " LOGTYPE-3");
			}
			else if (response.Contains(":GetContactAddress:"))
			{
				AuthenticatorQuery.HandleAction("GetContactAddress");
			}
			else
			{
				if (response.Contains("Server is not verified."))
				{
					return false;
				}
				Console.WriteLine("Could not update data on server list (legacy)- " + response + "LOGTYPE-4");
			}
			return true;
		}

		public static void HandleAction(string action)
		{
			if (!(action == "Restart"))
			{
				if (!(action == "RoundRestart"))
				{
					if (action == "UpdateData")
					{
						ServerConsole.Update = true;
						return;
					}
					if (action == "RefreshKey")
					{
						ServerConsole.RunRefreshPublicKeyOnce();
						return;
					}
					if (!(action == "GetContactAddress"))
					{
						return;
					}
					new Thread(new ThreadStart(AuthenticatorQuery.SendContactAddress))
					{
						Name = "SCP:SL Response to central servers (contact data request)",
						Priority = System.Threading.ThreadPriority.BelowNormal,
						IsBackground = true
					}.Start();
				}
				return;
			}
			Console.WriteLine("Server restart requested by central server.LOGTYPE-3");
		}

		public static void SaveNewToken(string token)
		{
			try
			{
				ServerConsole.VerKey = token;
				File.WriteAllText("./verkey.txt", token);
				Console.WriteLine("New verification token saved.LOGTYPE-3");
				ServerConsole.Update = true;
				ServerConsole.ScheduleTokenRefresh = true;
			}
			catch (Exception ex2)
			{
				Console.WriteLine("New verification token could not be saved (2): " + ex2.Message + "LOGTYPE-4");
			}
		}
	}
}
