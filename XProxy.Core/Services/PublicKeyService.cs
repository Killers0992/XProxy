﻿using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using XProxy.Cryptography;
using XProxy.Models;

namespace XProxy.Services
{
    public class PublicKeyService : BackgroundService
    {
        public HttpClient Client;
        public static AsymmetricKeyParameter PublicKey;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Add("User-Agent", "SCP SL");
            Client.DefaultRequestHeaders.Add("Game-Version", $"13.6.9");

            string text = CentralServerKeyCache.ReadCache();
            string text2 = string.Empty;
            string b = string.Empty;

            if (!string.IsNullOrEmpty(text))
            {
                PublicKey = ECDSA.PublicKeyFromString(text);
                text2 = Sha.HashToString(Sha.Sha256(ECDSA.KeyToString(PublicKey)));
                Logger.Debug(ConfigService.Singleton.Messages.LoadedPublicKeyFromCache, "PublicKeyService");
            }

            Logger.Debug(ConfigService.Singleton.Messages.DownloadPublicKeyFromCentrals, "PublicKeyService");
            while (!stoppingToken.IsCancellationRequested)
            {
                string responseText = "";
                try
                {
                    HttpResponseMessage response = await Client.GetAsync(string.Format("{0}v4/publickey.php?major={1}", "https://api.scpslgame.com/", "13"));
                    responseText = await response.Content.ReadAsStringAsync();

                    PublicKeyResponseModel publicKeyResponse = JsonConvert.DeserializeObject<PublicKeyResponseModel>(responseText);
                    if (!ECDSA.Verify(publicKeyResponse.Key, publicKeyResponse.Signature, CentralServerKeyCache.MasterKey))
                    {
                        Logger.Error(ConfigService.Singleton.Messages.CantRefreshPublicKeyMessage, "PublicKeyService");
                        await Task.Delay(360000);
                        continue;
                    }
                    PublicKey = ECDSA.PublicKeyFromString(publicKeyResponse.Key);
                    string text3 = Sha.HashToString(Sha.Sha256(ECDSA.KeyToString(PublicKey)));
                    if (text3 != b)
                    {
                        b = text3;
                        Logger.Debug(ConfigService.Singleton.Messages.ObtainedPublicKeyMessage, "PublicKeyService");
                        if (text3 != text2)
                        {
                            CentralServerKeyCache.SaveCache(publicKeyResponse.Key, publicKeyResponse.Signature);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ConfigService.Singleton.Messages.CantRefreshPublicKey2Message.Replace("%message%", ex.Message).Replace("%response%", responseText), "PublicKeyService");
                }
                await Task.Delay(360000);
            }
        }
    }
}
