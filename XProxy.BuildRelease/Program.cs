using IronZip;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Reflection;
using XProxy.Core;
using XProxy.Models;

await Host.CreateDefaultBuilder()
    .RunCommandLineApplicationAsync<AppCommand>(args);

[Command(Description = "Runs release builder.")]
public class AppCommand
{
    private HttpClient _http;
    public HttpClient Http
    {
        get
        {
            if (_http == null)
            {
                _http = new HttpClient();
                _http.DefaultRequestHeaders.UserAgent.ParseAdd("XProxy Release 1.0.0");
            }

            return _http;
        }
    }

    public string SlReferences => Environment.GetEnvironmentVariable("SL_REFERENCES");

    public string Workspace => Environment.GetEnvironmentVariable("GITHUB_WORKSPACE");
    public string MainPath => Path.Combine(Workspace, "main");

    public string XProxyCoreProject => Path.Combine(MainPath, "XProxy.Core");

    public async Task<int> OnExecute(IConsole console)
    {
        try
        {
            string releaseFolder = Path.Combine(XProxyCoreProject, "bin", "Release", "net9.0", "win-x64");

            string coreFile = Path.Combine(releaseFolder, "XProxy.Core.dll");

            if (!File.Exists(coreFile))
            {
                console.WriteLine($" [ERROR] XProxy.Core.dll not exists in location {coreFile}");
                return 1;
            }

            string targetCoreLocation = Path.Combine(MainPath, "XProxy.Core.dll");

            Assembly.LoadFrom(targetCoreLocation);

            File.Move(coreFile, targetCoreLocation);

            string[] refs =
            {
                "Assembly-CSharp.dll",
                "BouncyCastle.Cryptography.dll",
                "Mirror.dll",
                "NorthwoodLib.dll",
                "UnityEngine.CoreModule.dll"
            };

            List<string> validReferences = new List<string>();

            foreach(var file in refs)
            {
                string targetPath = Path.Combine(SlReferences, file);

                if (!File.Exists(targetPath))
                {
                    console.WriteLine($" [WARN] Can't find reference at {targetPath}");
                    continue;
                }
                
                validReferences.Add(targetPath);
            }

            console.WriteLine($" [INFO] Create dependencies.zip archive with\n-{string.Join("\n -",validReferences)}");

            using (var archive = new IronZipArchive(Path.Combine(MainPath, "dependencies.zip")))
            {
                foreach (var file in validReferences)
                    archive.Add(file);
            }

            console.WriteLine($" [INFO] Archive dependencies.zip created");

            BuildInfo bInfo = new BuildInfo()
            {
                DependenciesUrl = $"https://github.com/Killers0992/XProxy/releases/download/{BuildInformation.VersionText}/dependencies.zip",
                CoreUrl = $"https://github.com/Killers0992/XProxy/releases/download/{BuildInformation.VersionText}/XProxy.Core.dll",
                Changelogs = BuildInformation.Changelogs,
                Version = BuildInformation.VersionText,
                SupportedGameVersions = BuildInformation.SupportedGameVersions,
            };

            Environment.SetEnvironmentVariable("XPROXY_VERSION", BuildInformation.VersionText);

            string serialized = JsonConvert.SerializeObject(bInfo, Formatting.Indented);

            File.WriteAllText(Path.Combine(MainPath, "releaseinfo.json"), serialized);

            console.WriteLine($" [INFO] Serialized data to releaseinfo.json\n{serialized}");

            return 0;
        }
        catch (Exception ex)
        {
            console.WriteLine(ex);
            return 1;
        }
    }
}
