using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XProxy.Cryptography;
using System.IO;
using XProxy.Models;
using XProxy.Shared.Models;
using XProxy.Core;

namespace XProxy.Services
{
    public class ListService : BackgroundService
    {
        private ConfigService _config => ConfigService.Singleton;

        public static HttpClient Client;

        public static string PublicIp;
        public static string Password;
        public bool Update;
        public bool ScheduleTokenRefresh;
        public string VerKey;

        public static string Base64Decode(string base64EncodedData)
        {
            byte[] bytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(bytes);
        }

        public static string Base64Encode(string plainText)
        {
            byte[] array = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetMaxByteCount(plainText.Length));
            int bytes = Encoding.UTF8.GetBytes(plainText, 0, plainText.Length, array, 0);
            string result = Convert.ToBase64String(array, 0, bytes);
            ArrayPool<byte>.Shared.Return(array, false);
            return result;
        }

        public void RefreshToken(bool init = false)
        {
            Logger.Debug("Refresh Token");
            ScheduleTokenRefresh = false;

            if (!File.Exists(Path.Combine(ConfigService.MainDirectory, "verkey.txt")))
                File.WriteAllText(Path.Combine(ConfigService.MainDirectory, "verkey.txt"), "none");

            if (VerKey == null)
                VerKey = File.ReadAllText(Path.Combine(ConfigService.MainDirectory, "verkey.txt"));

            if (string.IsNullOrEmpty(VerKey))
                return;

            if (!init && string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(VerKey))
            {
                Logger.Info(_config.Messages.TokenLoadedMessage, $"ListService");
            }

            if (Password != VerKey)
            {
                Logger.Debug(_config.Messages.TokenReloadedMessage, $"ListService");
                Update = true;
            }

            Password = VerKey;
        }

        internal static readonly PlayerListSerializedModel PlayersListRaw = new PlayerListSerializedModel();
        internal static string _verificationPlayersList = string.Empty;

        public async Task<bool> SendData(Dictionary<string, string> data)
        {
            FormUrlEncodedContent content = new FormUrlEncodedContent(data);

            try
            {
                using (var response = await Client.PostAsync("https://api.scpslgame.com/v4/authenticator.php", content))
                {
                    string str = await response.Content.ReadAsStringAsync();

                    return (str.StartsWith("{\"") ? await ProcessResponse(str) : await ProcessLegacyResponse(str));
                }
            }
            catch(Exception ex)
            {
                Logger.Error(ex, "ListService");
                return false;
            }
        }

        public async Task<bool> ProcessResponse(string data)
        {
            AuthResponseModel authenticatorResponse = JsonConvert.DeserializeObject<AuthResponseModel>(data);
            if (!string.IsNullOrEmpty(authenticatorResponse.VerificationChallenge) && !string.IsNullOrEmpty(authenticatorResponse.VerificationResponse))
            {
                Logger.Info(_config.Messages.VerificationChallengeObtainedMessage, $"ListService");
            }
            if (!authenticatorResponse.Success)
            {
                Logger.Error(_config.Messages.FailedToUpdateMessage.Replace("%error%", authenticatorResponse.Error), $"ListService");
                return false;
            }
            else
            {
                if (!string.IsNullOrEmpty(authenticatorResponse.Token))
                {
                    Logger.Info(_config.Messages.ReceivedTokenMessage, $"ListService");
                    SaveNewToken(authenticatorResponse.Token);
                }
                if (authenticatorResponse.Actions != null && authenticatorResponse.Actions.Length != 0)
                {
                    string[] array = authenticatorResponse.Actions;
                    for (int i = 0; i < array.Length; i++)
                    {
                        await HandleAction(array[i]);
                    }
                }
                if (authenticatorResponse.Messages != null && authenticatorResponse.Messages.Length != 0)
                {
                    foreach (string str in authenticatorResponse.Messages)
                    {
                        Logger.Info(_config.Messages.MessageFromCentralsMessage.Replace("%message%", str), $"ListService");
                    }
                }
                return authenticatorResponse.Verified;
            }
        }

        public void SaveNewToken(string token)
        {
            try
            {
                VerKey = token;
                File.WriteAllText(Path.Combine(ConfigService.MainDirectory, "verkey.txt"), token);
                Logger.Info(_config.Messages.TokenSavedMessage, $"ListService");
                Update = true;
                ScheduleTokenRefresh = true;
            }
            catch (Exception ex)
            {
                Logger.Error(_config.Messages.TokenFailedToSaveMessage.Replace("%error%", ex.Message), $"ListService");
            }
        }

        public async Task<bool> ProcessLegacyResponse(string response)
        {
            if (response == "YES")
                return true;

            if (response.StartsWith("New code generated:"))
            {
                try
                {
                    string text = response.Remove(0, response.IndexOf(":", StringComparison.Ordinal)).Remove(response.IndexOf(":", StringComparison.Ordinal));
                    while (text.Contains(":"))
                    {
                        text = text.Replace(":", string.Empty);
                    }
                    VerKey = text;
                    File.WriteAllText(Path.Combine(ConfigService.MainDirectory, "verkey.txt"), text);
                    Logger.Info(_config.Messages.PasswordSavedMessage, $"ListService");
                    Update = true;
                    return true;
                }
                catch
                {
                    Logger.Error(_config.Messages.PasswordFailedToSaveMessage, $"ListService");
                    return true;
                }
            }
            if (response.Contains(":Restart:"))
            {
                await HandleAction("Restart");
            }
            else if (response.Contains(":RoundRestart:"))
            {
                await HandleAction("RoundRestart");
            }
            else if (response.Contains(":UpdateData:"))
            {
                await HandleAction("UpdateData");
            }
            else if (response.Contains(":RefreshKey:"))
            {
                await HandleAction("RefreshKey");
            }
            else if (response.Contains(":Message - "))
            {
                string text2 = response.Substring(response.IndexOf(":Message - ", StringComparison.Ordinal) + 11);
                text2 = text2.Substring(0, text2.IndexOf(":::", StringComparison.Ordinal));
                Logger.Info(_config.Messages.CentralCommandMessage.Replace("%message%", text2), $"CommandService");
            }
            else if (response.Contains(":GetContactAddress:"))
            {
                await HandleAction("GetContactAddress");
            }
            else
            {
                if (response.Contains("Server is not verified."))
                    return false;

                Logger.Error(_config.Messages.CantUpdateDataMessage, $"ListService");
            }
            return true;
        }

        public async Task HandleAction(string action)
        {
            switch (action.ToUpper())
            {
                case "RESTART":
                    break;
                case "ROUNDRESTART":
                    break;
                case "UPDATEDATA":
                    Update = true;
                    break;
                case "REFRESHKEY":
                    await RefreshPublicKeyOnce();
                    break;
                case "GETCONTACTADDRESS":
                    await SendContactAddress();
                    break;
            }
        }

        public async Task SendContactAddress()
        {
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "ip", PublicIp },
                { "port", $"{_config.Value.Port}" },
                { "version", "2" },
                { "address", Base64Encode(_config.Value.Email) }
            };

            if (!string.IsNullOrEmpty(Password))
                data.Add("passcode", Password);

            try
            {
                using (var response = await Client.PostAsync("https://api.scpslgame.com/v4/contactaddress.php", new FormUrlEncodedContent(data)))
                {
                    string text = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(text);
                }
            }
            catch(Exception ex)
            {
                Logger.Error(ex, "ListService");
            }
        }

        public async Task RefreshPublicKeyOnce()
        {
            try
            {
                using (var response = await Client.GetAsync("https://api.scpslgame.com/v4/publickey.php"))
                {
                    string text = await response.Content.ReadAsStringAsync();

                    PublicKeyResponseModel publicKeyResponse = JsonConvert.DeserializeObject<PublicKeyResponseModel>(text);
                    if (!ECDSA.Verify(publicKeyResponse.Key, publicKeyResponse.Signature, CentralServerKeyCache.MasterKey))
                    {
                        Logger.Error(_config.Messages.CantRefreshPublicKeyMessage, $"ListService");
                    }
                    else
                    {
                        PublicKeyService.PublicKey = ECDSA.PublicKeyFromString(publicKeyResponse.Key);
                        Logger.Debug(_config.Messages.ObtainedPublicKeyMessage, $"ListService");
                        CentralServerKeyCache.SaveCache(publicKeyResponse.Key, publicKeyResponse.Signature);
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Error(ex, "ListService");
            }
        }


        public async Task<string> GetPublicIp()
        {
            try
            {
                using (var response = await Client.GetAsync("https://api.scpslgame.com/ip.php"))
                {
                    string str = await response.Content.ReadAsStringAsync();

                    str = (str.EndsWith(".") ? str.Remove(str.Length - 1) : str);

                    return str;
                }
            }
            catch(Exception ex)
            {
                Logger.Error(ex, "ListService");
                return null;
            }
        }

        bool _verifyNotice = false;

        byte cycle;
        bool init = true;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Add("User-Agent", "SCP SL");
            Client.DefaultRequestHeaders.Add("Game-Version", ConfigModel.GameVersion);

            PublicIp = await GetPublicIp();

            RefreshToken(true);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DoCycle();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "ListService");
                }

                await Task.Delay(5000);

                if (ScheduleTokenRefresh || cycle == 0) RefreshToken();
            }
        }

        async Task DoCycle()
        {
            cycle += 1;
            if (!init && string.IsNullOrEmpty(Password) && cycle < 15)
            {
                if (cycle == 5 || cycle == 12 || ScheduleTokenRefresh)
                {
                    RefreshToken(false);
                }
            }
            else
            {
                init = false;
                Update = Update || cycle == 10;

                string str = JsonConvert.SerializeObject(new AuthPlayersModel());

                var serverWithUseSlots = _config.Value.Servers.FirstOrDefault(x => x.Value.UseSlotsForServerListPlayersCount);

                string playersStr = $"{ProxyService.Singleton.Players.Count}/{_config.Value.MaxPlayers}";

                if (serverWithUseSlots.Value != null)
                {
                    var targetServer = ProxyService.Singleton.GetServerByName(serverWithUseSlots.Key);
                    playersStr = $"{targetServer.PlayersOnline}/{targetServer.MaxPlayers}";
                }

                Dictionary<string, string> upd = Update ?
                    new Dictionary<string, string>()
                    {
                        { "ip", PublicIp },
                        { "players", playersStr },
                        { "playersList", _verificationPlayersList },
                        { "newPlayers", str },
                        { "port", $"{_config.Value.Port}" },
                        { "pastebin", _config.Value.Pastebin },
                        { "gameVersion", ConfigModel.GameVersion },
                        { "version", "2" },
                        { "update", "1" },
                        { "info", Base64Encode((_config.Value.MaintenanceMode ? PlaceHolders.ReplacePlaceholders(_config.Value.MaintenanceServerName) : PlaceHolders.ReplacePlaceholders(_config.Value.ServerName)).Replace('+', '-') + $"<color=#00000000><size=1>XProxy {ProxyBuildInfo.ReleaseInfo.Version}</size></color>") },
                        { "privateBeta", "false" },
                        { "staffRA", "false" },
                        { "friendlyFire", "false" },
                        { "geoblocking", "0" },
                        { "modded", "true" },
                        { "cgs", "true" },
                        { "whitelist", "false" },
                        { "accessRestriction", "false" },
                        { "emailSet", "true" },
                        { "enforceSameIp", "true" },
                        { "enforceSameAsn", "true" }
                    } :
                    new Dictionary<string, string>()
                    {
                        { "ip", PublicIp },
                        { "players", playersStr },
                        { "newPlayers", str },
                        { "port", $"{_config.Value.Port}" },
                        { "version", "2" },
                        { "enforceSameIp", "true" },
                        { "enforceSameAsn", "true" }
                    };

                if (!string.IsNullOrEmpty(Password))
                    upd.Add("passcode", Password);

                Update = false;

                bool result = await SendData(upd);

                if (result && !_verifyNotice)
                {
                    Logger.Info(_config.Messages.ServerListedMessage, "ListService");
                    _verifyNotice = true;
                }
            }

            if (cycle >= 15) 
                cycle = 0;
        }
    }
}
