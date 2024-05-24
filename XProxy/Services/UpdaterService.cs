using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using XProxy.Patcher.Services;
using XProxy.Services;
using XProxy.Shared.Models;

namespace XProxy.Shared.Services
{
    public class UpdaterService : BackgroundService
    {
        public static bool ForceReDownload;

        public static bool CheckForUpdates = true;

        string _buildsUrl = "https://killers0992.github.io/XProxy/builds.json";
        HttpClient _client;
        ConfigService _config;
        BuildInfo _latest = null;

        int seconds = 0;

        public UpdaterService(ConfigService config)
        {
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _client = new HttpClient();
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var response = await _client.GetAsync(_buildsUrl))
                {
                    string textResponse = await response.Content.ReadAsStringAsync();

                    ListingInfo listing = JsonConvert.DeserializeObject<ListingInfo>(textResponse);

                    BuildInfo[] latestBuildsForCurrentGameVersion = listing.Versions
                        .Where(x => x.Value.ParsedVersion.CompareTo(MainProcessService.AssemblyVersion) > 0 && x.Value.GameVersion == _config.Value.GameVersion)
                        .OrderByDescending(x => x.Value.ParsedVersion)
                        .Select(x => x.Value)
                        .ToArray();

                    BuildInfo firstLatest = latestBuildsForCurrentGameVersion.FirstOrDefault();

                    if (firstLatest != null)
                    {
                        if (_latest != null)
                        {
                            if (_latest.Version != firstLatest.Version)
                                _latest = firstLatest;
                        }
                        else
                            _latest = firstLatest;

                        Console.WriteLine($"Proxy is outdated, new version {_latest.Version}");
        
                        if (MainProcessService.IsWaitingForProcessExit)
                        {
                            Console.WriteLine("Waiting for process to exit...");
                            while (MainProcessService.IsWaitingForProcessExit)
                            {
                                await Task.Delay(10);
                            }
                        }

                        await DoUpdate();
                        CheckForUpdates = false;
                        MainProcessService.AssemblyUpdated = true;
                        MainProcessService.IsUpdating = false;
                    }
                    else
                    {
                        Console.WriteLine("Proxy is up to date!");
                        CheckForUpdates = false;
                    }
                }

                while(seconds < 30 && !ForceReDownload)
                {
                    await Task.Delay(1000);
                    seconds++;
                }
            }
        }

        async Task DoUpdate()
        {
            string targetBuildFile = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "XProxy_win64.zip" : "XProxy_linux64.zip";

            if (!_latest.Files.TryGetValue(targetBuildFile, out BuildFileInfo file))
                return;

            Logger.Info($"Downloading update...", "UpdaterService");

            string _tempFile = "_update.zip";

            using(var tempFileStream = new FileStream(_tempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                CustomProgressReporter reporter = new CustomProgressReporter("Downloading update %percentage%%...", null);

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

            Logger.Info("Update downloaded!");
        }
    }
}
