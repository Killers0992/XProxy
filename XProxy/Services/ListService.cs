namespace XProxy.Services;

public class ListService : BackgroundService
{
    public static HttpClient Client;

    public static string Password;
    public bool ScheduleTokenRefresh;
    public string VerKey;

    public void RefreshToken(bool init = false)
    {
        ScheduleTokenRefresh = false;

        if (!File.Exists("verkey.txt"))
            File.WriteAllText("verkey.txt", "none");

        if (VerKey == null)
            VerKey = File.ReadAllText("verkey.txt");

        if (string.IsNullOrEmpty(VerKey))
            return;

        if (Password != VerKey)
        {
            //Logger.Info("Token reloaded");

            foreach (Listener listener in Listener.List)
                listener.ForceServerListUpdate = true;
        }

        Password = VerKey;
    }

    public async Task<bool> SendData(Listener server, Dictionary<string, string> data)
    {
        FormUrlEncodedContent content = new FormUrlEncodedContent(data);

        try
        {
            using (var response = await server.Http.PostAsync("https://api.scpslgame.com/v4/authenticator.php", content))
            {
                string str = await response.Content.ReadAsStringAsync();

                return (str.StartsWith("{\"") ? await ProcessResponse(server, str) : await ProcessLegacyResponse(server, str));
            }
        }
        catch (Exception ex)
        {
            ProxyLogger.Error(ex, "ListService");
            return false;
        }
    }

    public async Task<bool> ProcessResponse(Listener server, string data)
    {
        AuthResponseModel authenticatorResponse = JsonConvert.DeserializeObject<AuthResponseModel>(data);

        if (!string.IsNullOrEmpty(authenticatorResponse.VerificationChallenge) && !string.IsNullOrEmpty(authenticatorResponse.VerificationResponse))
        {
            ProxyLogger.Info("Verificatio challenge obtained");
        }

        if (!authenticatorResponse.Success)
        {
            ProxyLogger.Error($"Failed to update {authenticatorResponse.Error}");
            return false;
        }
        else
        {
            if (!string.IsNullOrEmpty(authenticatorResponse.Token))
            {
                ProxyLogger.Info("Received token");
                SaveNewToken(authenticatorResponse.Token);
            }
            if (authenticatorResponse.Actions != null && authenticatorResponse.Actions.Length != 0)
            {
                string[] array = authenticatorResponse.Actions;
                for (int i = 0; i < array.Length; i++)
                {
                    await HandleAction(server, array[i]);
                }
            }
            if (authenticatorResponse.Messages != null && authenticatorResponse.Messages.Length != 0)
            {
                foreach (string str in authenticatorResponse.Messages)
                {
                    ProxyLogger.Info($"Message from central server {str}", "List");
                }
            }

            //Logger.Info("Is verified : " + authenticatorResponse.Verified);
            return authenticatorResponse.Verified;
        }
    }

    public void SaveNewToken(string token)
    {
        try
        {
            VerKey = token;
            File.WriteAllText("verkey.txt", token);

            ProxyLogger.Info("Token saved", $"ListService");

            foreach (Listener listener in Listener.List)
                listener.ForceServerListUpdate = true;

            ScheduleTokenRefresh = true;
        }
        catch (Exception ex)
        {
            ProxyLogger.Error("Token failed to save " + ex);
        }
    }

    public async Task<bool> ProcessLegacyResponse(Listener server, string response)
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

                File.WriteAllText("verkey.txt", text);

                ProxyLogger.Info("Password saved");

                server.ForceServerListUpdate = true;
                return true;
            }
            catch
            {
                ProxyLogger.Error("Failed to save password");
                return true;
            }
        }
        if (response.Contains(":Restart:"))
        {
            await HandleAction(server, "Restart");
        }
        else if (response.Contains(":RoundRestart:"))
        {
            await HandleAction(server, "RoundRestart");
        }
        else if (response.Contains(":UpdateData:"))
        {
            await HandleAction(server, "UpdateData");
        }
        else if (response.Contains(":RefreshKey:"))
        {
            await HandleAction(server, "RefreshKey");
        }
        else if (response.Contains(":Message - "))
        {
            string text2 = response.Substring(response.IndexOf(":Message - ", StringComparison.Ordinal) + 11);
            text2 = text2.Substring(0, text2.IndexOf(":::", StringComparison.Ordinal));
            ProxyLogger.Info(text2, $"CommandService");
        }
        else if (response.Contains(":GetContactAddress:"))
        {
            await HandleAction(server, "GetContactAddress");
        }
        else
        {
            if (response.Contains("Server is not verified."))
            {
                return false;
            }

            ProxyLogger.Error("Cant update data " + response, $"ListService");
        }
        return true;
    }

    public async Task HandleAction(Listener listener, string action)
    {
        ProxyLogger.Info(action);
        switch (action.ToUpper())
        {
            case "RESTART":
                break;
            case "ROUNDRESTART":
                break;
            case "UPDATEDATA":
                listener.ForceServerListUpdate = true;
                break;
            case "REFRESHKEY":
                //await RefreshPublicKeyOnce();
                break;
            case "GETCONTACTADDRESS":
                await SendContactAddress(listener);
                break;
        }
    }

    async Task SendContactAddress(Listener listener)
    {
        Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "ip", listener.PublicIp },
                { "port", $"{listener.ListenPort}" },
                { "version", "2" },
                { "address", listener.Settings.ServerList.Email.Base64Encode() }
            };

        if (!string.IsNullOrEmpty(Password))
            data.Add("passcode", Password);

        try
        {
            using (var response = await listener.Http.PostAsync("https://api.scpslgame.com/v4/contactaddress.php", new FormUrlEncodedContent(data)))
            {
                string text = await response.Content.ReadAsStringAsync();
                Console.WriteLine(text);
            }
        }
        catch (Exception ex)
        {
            ProxyLogger.Error(ex, "ListService");
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
                    ProxyLogger.Error("Cant refresh public key");
                }
                else
                {
                    PublicKeyService.Key = ECDSA.PublicKeyFromString(publicKeyResponse.Key);
                    ProxyLogger.Info("Obtained public key");
                    CentralServerKeyCache.SaveCache(publicKeyResponse.Key, publicKeyResponse.Signature);
                }
            }
        }
        catch (Exception ex)
        {
            ProxyLogger.Error(ex, "ListService");
        }
    }

    bool _verifyNotice = false;

    byte cycle;
    bool init = true;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Client = new HttpClient();
        Client.DefaultRequestHeaders.Add("User-Agent", "SCP SL");
        Client.DefaultRequestHeaders.Add("Game-Version", "14.1.3");

        RefreshToken(true);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DoCycle();
            }
            catch (Exception ex)
            {
                ProxyLogger.Error(ex, "ListService");
            }

            await Task.Delay(5000);

            if (ScheduleTokenRefresh || cycle == 0) RefreshToken();
        }
    }

    async Task DoCycle()
    {
        cycle += 1;

        foreach (Listener listener in Listener.List)
            listener.ServerListCycle += 1;

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

            foreach (Listener listener in Listener.List)
            {
                if (!listener.Settings.ServerList.ShowServerOnServerList)
                    continue;

                if (listener.PublicIp == "auto")
                    await listener.Initialize();

                listener.ServerListUpdate = listener.ForceServerListUpdate || listener.ServerListCycle == 10;

                string playersStr = $"{listener.ClientById.Values.Count}/{ProxySettings.Singleton.PlayerLimit}";

                Server target = Server.Get<Server>(name: listener.Settings.ServerList.TakePlayerCountFromServer);
                if (target != null)
                {
                    //playersStr = $"{target.Clients.Count}/25";
                }

                Dictionary<string, string> upd = listener.ServerListUpdate ?
                    new Dictionary<string, string>()
                    {
                        { "ip", listener.PublicIp },
                        { "players", playersStr },
                        { "playersList", string.Empty },
                        { "newPlayers", "\\{ objects: [] \\}" },
                        { "port", $"{listener.ListenPort}" },
                        { "pastebin", listener.Settings.ServerList.Pastebin },
                        { "gameVersion", listener.GameVersion.ToString(3) },
                        { "version", "2" },
                        { "update", "1" },
                        { "info", listener.Settings.Name.Replace('+', '-') + $"<color=#00000000><size=1>XProxy {BuildInformation.VersionText}</size></color>".Base64Encode() },
                        { "privateBeta", "False" },
                        { "staffRA", "False" },
                        { "friendlyFire", "False" },
                        { "geoblocking", "0" },
                        { "modded", "True" },
                        { "tmodded", "False" },
                        { "whitelist", "False" },
                        { "accessRestriction", "False" },
                        { "emailSet", "True" },
                        { "enforceSameIp", "True" },
                    } :
                    new Dictionary<string, string>()
                    {
                        { "ip", listener.PublicIp },
                        { "players", playersStr },
                        { "newPlayers", "\\{ objects: [] \\}" },
                        { "port", $"{listener.ListenPort}" },
                        { "version", "2" },
                    };

                if (!string.IsNullOrEmpty(Password))
                    upd.Add("passcode", Password);

                listener.ForceServerListUpdate = false;

                bool result = await SendData(listener, upd);

                if (result && !_verifyNotice)
                {
                    ProxyLogger.Info($"Server {listener.PublicIp}:{listener.ListenPort} is visible on list!");
                    _verifyNotice = true;
                }

                listener.ServerListUpdate = listener.ForceServerListUpdate || listener.ServerListCycle == 10;
            }
        }

        if (cycle >= 15)
            cycle = 0;

        foreach (Listener server in Listener.List)
        {
            if (server.ServerListCycle >= 15)
                server.ServerListCycle = 0;
        }
    }
}
