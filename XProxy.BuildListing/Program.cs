using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Octokit;
using System.ComponentModel.DataAnnotations;
using XProxy.Models;

await Host.CreateDefaultBuilder()
    .RunCommandLineApplicationAsync<AppCommand>(args);

[Command(Description = "Runs listing builder.")]
public class AppCommand
{
    public const float KillDamage = float.PositiveInfinity;

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

            BuildsListing listing = new BuildsListing();
            
            foreach (var release in releases)
            {
                foreach (var asset in release.Assets)
                {
                    string fileName = Path.GetFileName(asset.BrowserDownloadUrl);

                    switch (fileName.ToUpper())
                    {
                        case "RELEASEINFO.JSON":

                            var result = await Http.GetAsync(asset.BrowserDownloadUrl);

                            if (result.IsSuccessStatusCode)
                            {
                                var build = JsonConvert.DeserializeObject<BuildInfo>(await result.Content.ReadAsStringAsync());
                                listing.Builds.Add(build.Version, build);
                            }
                            break;
                    }
                }

                Console.WriteLine($"Add Version {release.TagName}");
            }

            File.WriteAllText("./Website/builds.json", JsonConvert.SerializeObject(listing, Formatting.Indented));
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
