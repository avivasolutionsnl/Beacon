using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;

using Beacon.Core.Models;

namespace Beacon.Core
{
    public class TeamCityMonitor
    {
        private readonly Config config;
        private readonly IBuildLight buildLight;
        private readonly HttpClient httpClient;

        public TeamCityMonitor(Config config, IBuildLight buildLight)
        {
            this.config = config;
            this.buildLight = buildLight;
            httpClient = new HttpClient(new HttpClientHandler
            {
                Credentials = new NetworkCredential(config.Username, config.Password)
            });

            httpClient.BaseAddress = new Uri(config.ServerUrl);
        }

        public async Task Start()
        {
            var buildTypeIds = config.BuildTypeIds == "*"
                ? new string[0]
                : config.BuildTypeIds.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).ToArray();

            buildLight.NoStatus();

            Console.CancelKeyPress += delegate { buildLight.NoStatus(); };

            while (true)
            {
                BuildStatus lastBuildStatus = await GetBuildStatus(buildTypeIds);

                switch (lastBuildStatus)
                {
                    case BuildStatus.Unavailable:
                        buildLight.NoStatus();
                        Logger.WriteLine("Build status not available");
                        break;

                    case BuildStatus.Passed:
                        buildLight.Success();
                        Logger.WriteLine("Passed");
                        break;

                    case BuildStatus.Investigating:
                        buildLight.Investigate();
                        Logger.WriteLine("Investigating");
                        break;

                    case BuildStatus.Failed:
                        buildLight.Fail();
                        Logger.WriteLine("Failed");
                        break;

                    case BuildStatus.Fixed:
                        buildLight.Fixed();
                        Logger.WriteLine("Fixed");
                        break;
                }

                Logger.Verbose($"Waiting for {config.Interval} seconds.");
                await Task.Delay(config.Interval);
            }
        }

        private async Task<BuildStatus> GetBuildStatus(IEnumerable<string> buildTypeIds)
        {
            BuildStatus status = BuildStatus.Unavailable;

            try
            {
                List<BuildStatus> results = await GetStatusOfAllBuilds(buildTypeIds.ToArray());
                if (!results.Any())
                {
                    status = BuildStatus.Unavailable;
                }
                else if (results.Any(result => result == BuildStatus.Failed))
                {
                    status = BuildStatus.Failed;
                }
                else if (results.Any(result => result == BuildStatus.Investigating))
                {
                    status = BuildStatus.Investigating;
                }
                else
                {
                    status = BuildStatus.Passed;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }

            return status;
        }

        private async Task<List<BuildStatus>> GetStatusOfAllBuilds(IEnumerable<string> buildTypeIds)
        {
            var statusPerBuild = new List<BuildStatus>();

            foreach (var buildTypeId in buildTypeIds)
            {
                HttpResponseMessage result = await httpClient.GetAsync(
                    $"httpAuth/app/rest/buildTypes/id:{buildTypeId}");

                if (result.IsSuccessStatusCode)
                {
                    var buildType = BuildType.FromXml(await result.Content.ReadAsStringAsync());

                    Logger.Verbose($"Analyzing the builds for '{buildType.Id}' over a period of {config.TimeSpan.Days} days.");

                    BuildStatus? status = await GetBuildTypeStatus(buildType);
                    if (status.HasValue)
                    {
                        statusPerBuild.Add(status.Value);
                    }

                    Logger.Verbose($"Status of Built Type '{buildType.Id}' is {status ?? BuildStatus.Unavailable}.");
                }
                else
                {
                    Logger.WriteLine($"Failed to get info for build type {buildTypeId}: {result.StatusCode}");
                }
            }

            return statusPerBuild;
        }

        private async Task<BuildStatus?> GetBuildTypeStatus(BuildType buildType)
        {
            BuildStatus? status = null;

            if (buildType.IsPaused)
            {
                Logger.Verbose("Bypassing because it is paused");
                return null;
            }

            var fromDate = DateTimeOffset.Now.Subtract(config.TimeSpan);
            string fromDateInTcFormat = Uri.EscapeDataString(fromDate.ToString("yyyyMMdd'T'HHmmssK").Replace(":", ""));
            
            string buildsXml = await httpClient.GetStringAsync(
                    $"httpAuth/app/rest/buildTypes/id:{buildType.Id}/builds?locator=branch:default:any,running:false,sinceDate:{fromDateInTcFormat}");

            var builds = BuildCollection.FromXml(buildsXml);
            if (builds.IsEmpty)
            {
                Logger.Verbose("Bypassing because no built history is available for it yet.");
                return null;
            }

            var dictionary = new Dictionary<string, Build>();
            foreach (Build build in builds)
            {
                string branch = build.BranchName;

                if (!dictionary.ContainsKey(branch) || dictionary[branch].Id < build.Id)
                {
                    dictionary[branch] = build;
                }
            }

            Build firstUnsuccesful = dictionary.Values.FirstOrDefault(v => !v.IsSuccessful);
            if (firstUnsuccesful == null)
            {
                return BuildStatus.Passed;
            }
            else
            {
                Logger.Verbose($"Build from branch {firstUnsuccesful.BranchName}(id: {firstUnsuccesful.Id}) failed");
            }

            if (buildType.IsUnstable)
            {
                Logger.Verbose("Bypassing because it is marked as unstable.");
                return null;
            }

            status = BuildStatus.Failed;

            Logger.Verbose("Now checking investigation status.");

            string investigationsXml =
                await httpClient.GetStringAsync($"/httpAuth/app/rest/investigations?locator=buildType:(id:{buildType.Id})");

            var investigation = Investigation.FromXml(investigationsXml);

            return investigation.Status ?? status;
        }
    }
}