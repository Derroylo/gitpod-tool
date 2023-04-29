using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;
using Octokit;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.IO.Compression;

namespace Gitpod.Tool.Helper
{
    class GptUpdateHelper
    {  
        private static async Task UpdateCacheFile()
        {
            GitHubClient client = new GitHubClient(new ProductHeaderValue("SomeName"));
            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("Derroylo", "gitpod-tool");
            IReadOnlyList<ReleaseAsset> assets = releases[0].Assets;

            JObject tmp = new JObject(
                new JProperty("last_check", DateTime.Now.ToString()),
                new JProperty("last_release", releases[0].TagName.Replace("v", "")),
                new JProperty("download_url", assets[0].BrowserDownloadUrl)
            );

            File.WriteAllText("releases.json", tmp.ToString());
        }

        public static async Task<string> GetLatestVersion(bool forceUpdate = false)
        {
            if (!File.Exists("releases.json") || forceUpdate) {
                await UpdateCacheFile();
            }

            JObject cacheFile = JObject.Parse(File.ReadAllText("releases.json"));

            // Check if we need to update the cache file
            var lastCheck = (DateTime) cacheFile["last_check"];
            var diffHours = (DateTime.Now - lastCheck).TotalHours;

            // If the last check is older then 4 hours, update the cache
            if (diffHours > 4) {
                await UpdateCacheFile();

                cacheFile = JObject.Parse(File.ReadAllText("releases.json"));
            }
            
            return (string) cacheFile["last_release"];
        }

        public static bool IsUpdateAvailable()
        {
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var latestVersion  = (GptUpdateHelper.GetLatestVersion()).Result;

            Version localVersion = new Version(currentVersion);
            Version latestRelease = new Version(latestVersion);

            int versionComparison = localVersion.CompareTo(latestRelease);

            if (versionComparison < 0) {
                return true;
            }
            
            return false;
        }

        public static async Task<bool> UpdateToLatestRelease()
        {
            JObject cacheFile = JObject.Parse(File.ReadAllText("releases.json"));
            var applicationDir = AppDomain.CurrentDomain.BaseDirectory;

            try {
                var httpClient = new HttpClient();
                var httpResult = await httpClient.GetAsync((string) cacheFile["download_url"]);
                using var resultStream = await httpResult.Content.ReadAsStreamAsync();
                using var fileStream = File.Create(applicationDir + "gitpod-tool.zip");

                resultStream.CopyTo(fileStream);
            } catch (Exception e) {
                AnsiConsole.WriteException(e);

                return false;
            }
            
            if (!File.Exists(applicationDir + "gitpod-tool.zip")) {
                AnsiConsole.WriteLine("Downloading the latest release failed");

                return false;
            }

            try {
                ZipFile.ExtractToDirectory(applicationDir + "gitpod-tool.zip", applicationDir + "update", true);
            } catch (Exception e) {
                AnsiConsole.WriteException(e);

                return false;
            }

            return true;
        }
    }
}