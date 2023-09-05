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
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.IO.Compression;
using Semver;

namespace Gitpod.Tool.Helper
{
    class GptUpdateHelper
    {  
        private static async Task UpdateCacheFile()
        {
            var applicationDir = AppDomain.CurrentDomain.BaseDirectory;

            GitHubClient client = new GitHubClient(new ProductHeaderValue("SomeName"));
            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("Derroylo", "gitpod-tool");

            bool allowPreReleases = GptConfigHelper.Config.Config.AllowPreReleases;
            Release lastRelease = null;

            foreach (Release release in releases) {
                if (release.Draft) {
                    continue;
                }

                if (!allowPreReleases && release.Prerelease) {
                    continue;
                }

                if (null == lastRelease) {
                    lastRelease = release;
                }
            }

            IReadOnlyList<ReleaseAsset> assets = lastRelease.Assets;

            JObject tmp = new JObject(
                new JProperty("last_check", DateTime.Now.ToString()),
                new JProperty("last_release", lastRelease.TagName.Replace("v", "")),
                new JProperty("download_url", assets[0].BrowserDownloadUrl)
            );

            File.WriteAllText(applicationDir + "releases.json", tmp.ToString());
        }

        public static async Task<string> GetLatestVersion(bool forceUpdate = false)
        {
            var applicationDir = AppDomain.CurrentDomain.BaseDirectory;

            if (!File.Exists(applicationDir + "releases.json") || forceUpdate) {
                await UpdateCacheFile();
            }

            JObject cacheFile = JObject.Parse(File.ReadAllText(applicationDir + "releases.json"));

            // Check if we need to update the cache file
            var lastCheck = (DateTime) cacheFile["last_check"];
            var diffHours = (DateTime.Now - lastCheck).TotalHours;

            // If the last check is older then 4 hours, update the cache
            if (diffHours > 4) {
                await UpdateCacheFile();

                cacheFile = JObject.Parse(File.ReadAllText(applicationDir + "releases.json"));
            }
            
            return (string) cacheFile["last_release"];
        }

        public static bool IsUpdateAvailable()
        {
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var latestVersion  = (GptUpdateHelper.GetLatestVersion()).Result;

            SemVersion localVersion = SemVersion.Parse(currentVersion, SemVersionStyles.Strict);
            SemVersion latestRelease = SemVersion.Parse(latestVersion, SemVersionStyles.Strict);

            int versionComparison = localVersion.CompareSortOrderTo(latestRelease);

            if (versionComparison < 0) {
                return true;
            }
            
            return false;
        }

        public static async Task<bool> UpdateToLatestRelease()
        {
            var applicationDir = AppDomain.CurrentDomain.BaseDirectory;
            var newGptDir = "/workspace/.gpt";

            JObject cacheFile = JObject.Parse(File.ReadAllText(applicationDir + "releases.json"));

            try {
                if (!Directory.Exists(newGptDir)) {
                    Directory.CreateDirectory(newGptDir);
                }
            } catch (Exception e) {
                AnsiConsole.WriteException(e);

                return false;
            }

            try {
                var httpClient = new HttpClient();
                var httpResult = await httpClient.GetAsync((string) cacheFile["download_url"]);
                using var resultStream = await httpResult.Content.ReadAsStreamAsync();
                using var fileStream = File.Create(newGptDir + "gitpod-tool.zip");

                resultStream.CopyTo(fileStream);
            } catch (Exception e) {
                AnsiConsole.WriteException(e);

                return false;
            }
            
            if (!File.Exists(newGptDir + "gitpod-tool.zip")) {
                AnsiConsole.WriteLine("Downloading the latest release failed");

                return false;
            }

            try {
                ZipFile.ExtractToDirectory(newGptDir + "gitpod-tool.zip", newGptDir + "update", true);
            } catch (Exception e) {
                AnsiConsole.WriteException(e);

                return false;
            }

            return true;
        }
    }
}