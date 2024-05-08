using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Octokit;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using XProxy.Models;

await Host.CreateDefaultBuilder()
    .RunCommandLineApplicationAsync<AppCommand>(args);

[Command(Description = "Runs builder.")]
public class AppCommand
{
    private GitHubClient _gitHubClient;
    public GitHubClient Client
    {
        get
        {
            if (_gitHubClient == null)
            {
                _gitHubClient = new GitHubClient(new ProductHeaderValue("XProxy.Listing"));
                _gitHubClient.Credentials = new Credentials(Token);
            }

            return _gitHubClient;
        }
    }

    private HttpClient _http;
    public HttpClient Http
    {
        get
        {
            if (_http == null)
            {
                _http = new HttpClient();
                _http.DefaultRequestHeaders.UserAgent.ParseAdd("XProxy Listing 1.0.0");
            }

            return _http;
        }
    }

    public string Repository => Environment.GetEnvironmentVariable("GITHUB_REPOSITORY");
    public string RepositoryOwner => Environment.GetEnvironmentVariable("GITHUB_REPOSITORY_OWNER");

    string BytesToMD5(byte[] bytes)
    {
        using (var md5 = MD5.Create())
        {
            return BitConverter.ToString(md5.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }
    }

    [Required]
    [Option(Description = "Build.")]
    public string Token { get; set; } = null;

    public async Task<int> OnExecute(IConsole console)
    {
        try
        {
            string[] sp = Repository.Split("/");

            string repositoryName = sp[1];

            var releases = await Client.Repository.Release.GetAll(RepositoryOwner, repositoryName);

            if (releases.Count == 0)
            {
                Console.WriteLine("0 releases found!");
                return 1;
            }

            ListingInfo lInfo = new ListingInfo();

            foreach(var release in releases)
            {
                ReleaseInfo rInfo = null;

                Dictionary<string, BuildFileInfo> files = new Dictionary<string, BuildFileInfo>();

                foreach (var asset in release.Assets)
                {
                    string fileName = Path.GetFileName(asset.BrowserDownloadUrl);

                    switch (fileName.ToUpper())
                    {
                        case "RELEASEINFO.JSON":

                            var result = await Http.GetAsync(asset.BrowserDownloadUrl);

                            if (result.IsSuccessStatusCode)
                                rInfo = JsonConvert.DeserializeObject<ReleaseInfo>(await result.Content.ReadAsStringAsync());
                            break;
                        default:
                            var result2 = await Http.GetAsync(asset.BrowserDownloadUrl);

                            if (result2.IsSuccessStatusCode)
                            {
                                byte[] bytes = await result2.Content.ReadAsByteArrayAsync();
                                files.Add(fileName, new BuildFileInfo()
                                {
                                    Hash = BytesToMD5(bytes),
                                    Name = fileName,
                                    Url = asset.BrowserDownloadUrl,
                                });
                            }
                            break;
                    }
                }

                if (rInfo == null) continue;

                BuildInfo bInfo = new BuildInfo()
                {
                    GameVersion = rInfo.GameVersion,
                    Version = rInfo.Version,
                    Files = files,
                };

                if (!lInfo.Versions.ContainsKey(release.TagName))
                    lInfo.Versions.Add(release.TagName, bInfo);
            }

            File.WriteAllText("./Website/builds.json", JsonConvert.SerializeObject(lInfo, Formatting.Indented));
            Console.WriteLine("Builds file uploaded!");
            return 0;
        }
        catch (Exception ex)
        {
            console.WriteLine(ex);
            return 1;
        }
    }
}
