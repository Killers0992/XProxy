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

        bool upToDateNotify = false;
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

                        Logger.Info($"Proxy is outdated (f=green){MainProcessService.AssemblyVersion.ToString(3)}(f=white), new version (f=green){_latest.Version}(f=white)", "XProxy");
        
                        if (MainProcessService.IsWaitingForProcessExit)
                        {
                            Logger.Info("If you want to update now press (f=red)CTRL+C(f=white) ( kill process )", "XProxy");
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
                    else if (!upToDateNotify)
                    {
                        Logger.Info("Proxy is up to date!", "XProxy");
                        upToDateNotify = true;
                        CheckForUpdates = false;
                    }
                }

                while(seconds < 30 && !MainProcessService.IsUpdating)
                {
                    await Task.Delay(1000);
                    seconds++;
                }

                seconds = 0;
            }
        }

        async Task DoUpdate()
        {
            string targetBuildFile = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "XProxy_win64.zip" : "XProxy_linux64.zip";

            if (!_latest.Files.TryGetValue(targetBuildFile, out BuildFileInfo file))
                return;

            if (!Directory.Exists("Core"))
                Directory.CreateDirectory("Core");

            else
            {
                Directory.Delete("Core", true);
                Directory.CreateDirectory("Core");
            }

            Logger.Info($"Downloading update...", "XProxy");

            string _tempFile = "_update.zip";

            using(var tempFileStream = new FileStream(_tempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                CustomProgressReporter reporter = new CustomProgressReporter("Downloading update (f=green)%percentage%%(f=white)...", null);

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
                            if (!Directory.Exists(Path.Combine(MainProcessService.CoreFolder, name)))
                                Directory.CreateDirectory(Path.Combine(MainProcessService.CoreFolder, name));
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

                            bool fileExists = File.Exists(Path.Combine(MainProcessService.CoreFolder, name));

                            if (fileExists)
                            {
                                using (var fileStream = File.OpenRead(Path.Combine(MainProcessService.CoreFolder, name)))
                                {
                                    targetHash = Convert.ToBase64String(md5.ComputeHash(fileStream));
                                }
                            }

                            if (hash != targetHash)
                            {
                                try
                                {
                                    File.WriteAllBytes(Path.Combine(MainProcessService.CoreFolder, name), data);
                                }
                                catch (Exception) { }
                            }
                        }
                    }
                }
            }

            Logger.Info("Update downloaded!", "XProxy");
        }
    }
}
