using Newtonsoft.Json;
using XProxy.Misc;
using XProxy.Models;

namespace XProxy.Services;

public class PublicKeyService : BackgroundService
{
    public HttpClient Client;
    public static AsymmetricKeyParameter Key;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Client = new HttpClient();
        Client.DefaultRequestHeaders.Add("User-Agent", "SCP SL");
        Client.DefaultRequestHeaders.Add("Game-Version", $"14.1.0");

        string text = CentralServerKeyCache.ReadCache();

        string text2 = string.Empty;

        string b = string.Empty;

        if (!string.IsNullOrEmpty(text))
        {
            Key = ECDSA.PublicKeyFromString(text);
            text2 = Sha.HashToString(Sha.Sha256(ECDSA.KeyToString(Key)));
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            string responseText = "";
            try
            {
                HttpResponseMessage response = await Client.GetAsync(string.Format("{0}v4/publickey.php?major={1}", "https://api.scpslgame.com/", "14"));
                responseText = await response.Content.ReadAsStringAsync();

                PublicKey publicKeyResponse = JsonConvert.DeserializeObject<PublicKey>(responseText);

                if (!ECDSA.Verify(publicKeyResponse.Key, publicKeyResponse.Signature, CentralServerKeyCache.MasterKey))
                {
                    await Task.Delay(360000);
                    continue;
                }

                Key = ECDSA.PublicKeyFromString(publicKeyResponse.Key);
                string hashKey = Sha.HashToString(Sha.Sha256(ECDSA.KeyToString(Key)));

                if (hashKey != b)
                {
                    b = hashKey;
                    if (hashKey != text2)
                        CentralServerKeyCache.SaveCache(publicKeyResponse.Key, publicKeyResponse.Signature);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "PublicKeyService");
            }

            await Task.Delay(360000);
        }
    }
}
