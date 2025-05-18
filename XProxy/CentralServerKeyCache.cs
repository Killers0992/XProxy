namespace XProxy;

public static class CentralServerKeyCache
{
    public static string ReadCache()
    {
        string result;
        try
        {
            if (!File.Exists("./centralcache.txt"))
            {
                //Logger.Info($"Central server public key not found in cache.", $"CentralServerKeyCache");
                result = null;
            }
            else if (!File.Exists("./centralkeysignature.txt"))
            {
                //Logger.Info($"Central server public key signature not found in cache.", $"CentralServerKeyCache");
                result = null;
            }
            else
            {
                string[] source = File.ReadAllLines("./centralcache.txt");
                string[] array = File.ReadAllLines("./centralkeysignature.txt");
                if (array.Length == 0)
                {
                    //Logger.Error($"Can't load central server public key from cache - empty signature.", $"CentralServerKeyCache");
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
                            //Logger.Error($"Invalid signature of Central Server Key in cache!", $"CentralServerKeyCache");
                            result = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        //Logger.Error($"Can't load central server public key from cache - " + ex.Message, $"CentralServerKeyCache");
                        result = null;
                    }
                }
            }
        }
        catch (Exception ex2)
        {

            //Logger.Error($"Can't read public key cache - " + ex2.Message, $"CentralServerKeyCache");
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
                //Logger.Error($"Invalid signature of Central Server Key!", $"CentralServerKeyCache");
            }
            else
            {
                if (File.Exists("./centralcache.txt"))
                {
                    if (key == CentralServerKeyCache.ReadCache())
                    {
                        //Logger.Info($"Key cache is up to date.", $"CentralServerKeyCache");
                        return;
                    }
                    File.Delete("./centralcache.txt");
                }

                //Logger.Info($"Updating key cache...", $"CentralServerKeyCache");
                File.WriteAllText($"./centralcache.txt", key, Encoding.UTF8);
                File.WriteAllText($"./centralkeysignature.txt", signature, Encoding.UTF8);
                //Logger.Info($"Key cache updated!", $"CentralServerKeyCache");
            }
        }
        catch (Exception ex)
        {
            //Logger.Error("Can't write public key cache - " + ex.Message, $"CentralServerKeyCache");
        }
    }

    public static readonly AsymmetricKeyParameter MasterKey = Cryptography.ECDSA.PublicKeyFromString("-----BEGIN PUBLIC KEY-----\r\nMIGbMBAGByqGSM49AgEGBSuBBAAjA4GGAAQAbL0YvrhVB2meqCq5XzjAJD8Ii0hb\r\nBHdIQ587N583cP8twjDhcITjZhBHJPJDuA85XdpgG04HwT0SD3WcAvoQXBUAUsG1\r\nLS9TR4urHwfgfroq4tH2HAQE6ZxFZeIFSglLO8nxySim4yKBj96HLG624lzKvzoD\r\nId+GOwjcd3XskOq9Dwc=\r\n-----END PUBLIC KEY-----");
}