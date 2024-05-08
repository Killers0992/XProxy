using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using XProxy.Models;

namespace XProxy.Services
{
    public class UpdaterService : BackgroundService
    {
        string _buildsUrl = "https://killers0992.github.io/XProxy/builds.json";
        HttpClient _client;
        ConfigService _config;
        BuildInfo _latest = null;
        bool notifyUpToDate = false;

        public UpdaterService(ConfigService config)
        {
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _client = new HttpClient();
            while (true)
            {
                using (var response = await _client.GetAsync(_buildsUrl))
                {
                    string textResponse = await response.Content.ReadAsStringAsync();

                    ListingInfo listing = JsonConvert.DeserializeObject<ListingInfo>(textResponse);

                    BuildInfo[] latestBuildsForCurrentGameVersion = listing.Versions.Where(x => x.Value.ParsedVersion.CompareTo(ProxyBuildInfo.Version) > 0 && x.Value.GameVersion == _config.Value.GameVersion).Select(x => x.Value).ToArray();

                    BuildInfo firstLatest = latestBuildsForCurrentGameVersion.FirstOrDefault();

                    if (firstLatest == null)
                    {
                        if (!notifyUpToDate)
                        {
                            Logger.Info(_config.Messages.ProxyIsUpToDate, "UpdaterService");
                            notifyUpToDate = true;
                        }
                    }
                    else
                    {
                        bool sendNotifyMessage = false;

                        if (_latest != null)
                        {
                            if (_latest.Version != firstLatest.Version)
                            {
                                _latest = firstLatest;
                                sendNotifyMessage = true;
                            }
                        }
                        else
                        {
                            _latest = firstLatest;
                            sendNotifyMessage = true;
                        }

                        if (sendNotifyMessage)
                            Logger.Info(_config.Messages.ProxyIsOutdated.Replace("%version%", _latest.Version), "UpdaterService");

                        if (ProxyService.Singleton.Players.Count == 0 && _config.Value.AutoUpdater)
                            await DoUpdate();
                    }
                }

                await Task.Delay(30000);
            }
        }

        async Task DoUpdate()
        {
            string targetBuildFile = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "XProxy_win64.zip" : "XProxy_linux64.zip";

            if (!_latest.Files.TryGetValue(targetBuildFile, out BuildFileInfo file))
                return;

            ProxyService.Singleton._config.Value.MaintenanceMode = true;
            Logger.Info(_config.Messages.DownloadingUpdate.Replace("%percentage%", "0"), "UpdaterService");

            string _tempFile = "_update.zip";

            using(var tempFileStream = new FileStream(_tempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                CustomProgressReporter reporter = new CustomProgressReporter(_config.Messages.DownloadingUpdate, "UpdaterService");

                await _client.DownloadAsync(file.Url, tempFileStream, reporter);

                int currentEntry = 0;
                var md5 = MD5.Create();

                using (ZipArchive zip = new ZipArchive(tempFileStream))
                {
                    foreach (var entry in zip.Entries)
                    {
                        currentEntry++;

                        string name = entry.FullName;

                        bool isDirectory = string.IsNullOrEmpty(Path.GetExtension(name)) && name.EndsWith("/");

                        if (name.EndsWith("/"))
                            name = name.Substring(0, name.Length - 1);

                        if (isDirectory)
                        {
                            if (!Directory.Exists(name))
                                Directory.CreateDirectory(name);
                        }
                        else
                        {
                            var stream = entry.Open();                               
                            byte[] buffer = new byte[16*1024];

                            byte[] data = null;

                            using (MemoryStream ms = new MemoryStream())
                            {
                                int read;
                                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    ms.Write(buffer, 0, read);
                                }

                                data = ms.ToArray();
                            }

                            string hash = Convert.ToBase64String(md5.ComputeHash(data));

                            string targetHash = null;

                            bool fileExists = File.Exists(name);

                            if (fileExists)
                            {
                                using (var fileStream = File.OpenRead(name))
                                {
                                    targetHash = Convert.ToBase64String(md5.ComputeHash(fileStream));
                                }
                            }

                            if (hash != targetHash)
                            {
                                try
                                {
                                    File.WriteAllBytes(name, data);
                                }
                                catch (Exception) { }
                            }
                        }
                    }
                }
            }

            Logger.Info(_config.Messages.DownloadedUpdate, "UpdaterService");

            Process.GetCurrentProcess().Kill();
        }
    }
}
