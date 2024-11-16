using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using XProxy.Misc;
using XProxy.Models;

namespace XProxy.Services
{
    public class UpdaterService : BackgroundService
    {
        private static bool _uptodateNotice;
        private static HttpClient _client = new HttpClient();
        private static BuildInfo _latestBuild;

        public const string BuildsProviderUrl = "https://killers0992.github.io/XProxy/builds.json";
        public const string VersionsUrls = "https://raw.githubusercontent.com/Killers0992/XProxy/master/Storage/gameVersions.json";

        public const int CheckUpdatesEveryMs = 30000;

        public static Version InstalledVersion;
        public static string[] RemoteGameVersions;

        public static string DependenciesFolder => Path.Combine(Environment.CurrentDirectory, "Dependencies");
        public static string ProxyFile => Path.Combine(Environment.CurrentDirectory, "XProxy.Core.dll");

        public static void FetchVersion()
        {
            if (!File.Exists(ProxyFile))
            {
                InstalledVersion = new Version(0,0,0);
                return;
            }

            AssemblyName name = AssemblyName.GetAssemblyName(ProxyFile);

            if (name == null)
            {
                InstalledVersion = new Version(0, 0, 0);
                return;
            }

            InstalledVersion = name.Version;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(CheckUpdatesEveryMs);

                try
                {
                    await CheckUpdates();
                }
                catch (Exception ex)
                {
                    ConsoleLogger.Error($"Failed checking updates {ex}", "XProxy");
                }
            }
        }

        public static async Task IntialRun()
        {
            FetchVersion();
            await CheckUpdates(true);
        }

        public static async Task CheckUpdates(bool isIntial = false)
        {
            using (var response = await _client.GetAsync(VersionsUrls))
            {
                string textResponse = await response.Content.ReadAsStringAsync();

                string[] versions = JsonConvert.DeserializeObject<string[]>(textResponse);

                if (versions != null)
                    RemoteGameVersions = versions;
            }

            BuildsListing listing = null;
            try
            {
                listing = await FetchBuilds();
            }
            catch (HttpRequestException ex)
            {
                ConsoleLogger.Error($"Failed to fetch builds ({(ex.StatusCode.HasValue ? ex.StatusCode.Value : $"Website Down")}) !", "XProxy");
            }

            if (listing == null)
            {
                ConsoleLogger.Error($"Fetched listing is invalid!", "XProxy");
                return;
            }

            BuildInfo[] builds = listing.Builds
                .Where(x => x.Value.ParsedVersion.CompareTo(InstalledVersion) > 0 && x.Value.SupportedGameVersions.Contains(LauncherSettings.Value.GameVersion.ToUpper() == "LATEST" ? RemoteGameVersions[0] : LauncherSettings.Value.GameVersion))
                .OrderByDescending(x => x.Value.ParsedVersion)
                .Select(x => x.Value)
                .ToArray();

            if (builds.Length == 0)
            {
                if (!_uptodateNotice && InstalledVersion.Major != 0)
                {
                    ConsoleLogger.Info("Proxy is up to date!", "XProxy");
                    _uptodateNotice = true;
                }
            }
            else
            {
                BuildInfo latest = builds.FirstOrDefault();

                if (latest != _latestBuild)
                {
                    if (InstalledVersion.Major == 0)
                        ConsoleLogger.Info($"Installing (f=cyan)XProxy(f=white) ((f=green){latest.ParsedVersion.ToString(3)}(f=white))", "XProxy");
                    else
                        ConsoleLogger.Info($"New version of (f=cyan)XProxy(f=white) found, (f=darkgreen){InstalledVersion.ToString(3)}(f=white) => (f=green){latest.ParsedVersion.ToString(3)}(f=white)", "XProxy");

                    if (latest.Changelogs.Length == 0)
                        ConsoleLogger.Info("Changelogs not found...", "XProxy");
                    else
                    {
                        ConsoleLogger.Info("Changelogs:", "XProxy");
                        foreach (var changelog in  latest.Changelogs)
                        {
                            ConsoleLogger.Info($" - (f=green){changelog}(f=white)");
                        }
                    }

                    _latestBuild = latest;
                }

                if (isIntial)
                {
                    if (LauncherSettings.Value.DisableUpdater)
                    {
                        ConsoleLogger.Warn($"Updater is disabled, skipping build download!", "XProxy");
                    }
                    else
                        await DownloadBuild(_latestBuild);
                }
            }
        }

        private static async Task<BuildsListing> FetchBuilds()
        {
            using (HttpResponseMessage response = await _client.GetAsync(BuildsProviderUrl))
            {
                string textResponse = await response.Content.ReadAsStringAsync();

                try
                {
                    BuildsListing listing = JsonConvert.DeserializeObject<BuildsListing>(textResponse);
                    return listing;
                }
                catch(Exception ex)
                {
                    ConsoleLogger.Error($"Failed deserializing builds listing! {ex}", "XProxy");
                    return null;
                }
            }
        }

        private static async Task DownloadBuild(BuildInfo build)
        {
            using (FileStream proxyStream = new FileStream(ProxyFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                CustomProgressReporter reporter = new CustomProgressReporter("Downloading (f=cyan)XProxy.Core.dll(f=white) (f=green)%percentage%%(f=white)...", null);

                await _client.DownloadAsync(build.CoreUrl, proxyStream, reporter);
            }

            FetchVersion();
            ConsoleLogger.Info($"Downloaded (f=cyan)XProxy.Core.dll(f=white) ((f=green){InstalledVersion.ToString(3)}(f=white))", "XProxy");

            string _dependencies = "./_dependencies.zip";

            if (!Directory.Exists(DependenciesFolder))
                Directory.CreateDirectory(DependenciesFolder);

            using (FileStream depsStream = new FileStream(_dependencies, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                CustomProgressReporter reporter = new CustomProgressReporter("Downloading dependencies (f=green)%percentage%%(f=white)...", null);

                await _client.DownloadAsync(build.DependenciesUrl, depsStream, reporter);

                using (ZipArchive depsZip = new ZipArchive(depsStream))
                {
                    Dictionary<string, string> dependenciesHashes = GetDependenciesHashes();
                    List<string> dependenciesToRemove = dependenciesHashes.Values.ToList();

                    MD5 md5 = MD5.Create();

                    foreach (ZipArchiveEntry entry in depsZip.Entries)
                    {
                        byte[] buffer = new byte[16*1024];

                        byte[] data = null;

                        Stream stream = entry.Open();

                        using (MemoryStream ms = new MemoryStream())
                        {
                            int read;
                            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                ms.Write(buffer, 0, read);
                            }

                            data = ms.ToArray();
                        }

                        string entryHash = Convert.ToBase64String(md5.ComputeHash(data));

                        string entryName = entry.Name;

                        if (dependenciesHashes.TryGetValue(entryName, out string depHash))
                        {
                            if (depHash == entryHash)
                            {
                                // This dependency is uptodate! skip
                                dependenciesToRemove.Remove(entryName);
                                continue;
                            }
                            else
                            {
                                // This dependency neets to be updated!
                                File.WriteAllBytes(Path.Combine(DependenciesFolder, entryName), data);
                                dependenciesToRemove.Remove(entryName);
                                ConsoleLogger.Info($"Update dependency (f=cyan){entryName}(f=white)", "XProxy");
                            }
                        }
                        else
                        {
                            // This dependency not exists, create one!
                            File.WriteAllBytes(Path.Combine(DependenciesFolder, entryName), data);
                            ConsoleLogger.Info($"Download dependency (f=cyan){entryName}(f=white)", "XProxy");
                        }
                    }

                    foreach(var dependencyToRemove in dependenciesToRemove)
                    {
                        string targetPath = Path.Combine(DependenciesFolder, dependencyToRemove);

                        if (!File.Exists(targetPath))
                            continue;

                        File.Delete(targetPath);
                    }
                }
            }

            ConsoleLogger.Info($"Downloaded (f=cyan)XProxy(f=white) dependencies", "XProxy");
        }

        private static Dictionary<string, string> GetDependenciesHashes()
        {
            MD5 md5 = MD5.Create();

            Dictionary<string, string> hashes = new Dictionary<string, string>();

            foreach (var file in Directory.GetFiles(DependenciesFolder))
            {
                byte[] hashAlgo = md5.ComputeHash(File.ReadAllBytes(file));

                string name = Path.GetFileName(file);

                hashes[name] = Convert.ToBase64String(hashAlgo);
            }

            return hashes;
        }
    }
}
