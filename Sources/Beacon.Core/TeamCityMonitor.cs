using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Beacon.Core.Models;

namespace Beacon.Core
{
    public class TeamCityMonitor : BuildMonitor
    {
        private readonly string authPath;
        private readonly TeamcityConfig teamcityConfig;
        private readonly HttpClient httpClient;
        private readonly IEnumerable<string> buildTypeIds;

        public TeamCityMonitor(TeamcityConfig teamcityConfig, IBuildLight buildLight) : base(buildLight, teamcityConfig)
        {
            this.teamcityConfig = teamcityConfig;
            authPath = teamcityConfig.GuestAccess ? "guestAuth" : "httpAuth";

            var httpClientHandler = new HttpClientHandler
            {
                Credentials = new NetworkCredential(teamcityConfig.Username, teamcityConfig.Password)
            };

            httpClient = new HttpClient(httpClientHandler)
            {
                BaseAddress = new Uri(teamcityConfig.ServerUrl)
            };
            
            buildTypeIds = config.BuildTypeIds == "*"
                ? new string[0]
                : config.BuildTypeIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
        }

        protected override async Task<BuildStatus> GetBuildStatus()
        {
            try
            {
                List<BuildStatus> results = await GetStatusOfAllBuilds();

                return CombineStatuses(results);
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }

            return BuildStatus.Unavailable;
        }

        private async Task<List<BuildStatus>> GetStatusOfAllBuilds()
        {
            var statusPerBuild = new List<BuildStatus>();

            foreach (var buildTypeId in buildTypeIds)
            {
                HttpResponseMessage result = await httpClient.GetAsync($"{authPath}/app/rest/buildTypes/id:{buildTypeId}");

                if (result.IsSuccessStatusCode)
                {
                    var buildType = BuildType.FromXml(await result.Content.ReadAsStringAsync());

                    Logger.Verbose($"Analyzing the builds for '{buildType.Id}' over a period of {teamcityConfig.TimeSpan.Days} days.");

                    BuildStatus? status = await GetBuildTypeStatus(buildType);
                    if (status.HasValue)
                    {
                        statusPerBuild.Add(status.Value);
                    }

                    var statusMessage = $"Status of build type '{buildType.Id}' is {status ?? BuildStatus.Unavailable}.";
                    switch (status)
                    {
                        case BuildStatus.Failed:
                            Logger.WriteErrorLine(statusMessage);
                            break;
                        case BuildStatus.Investigating:
                            Logger.WriteWarningLine(statusMessage);
                            break;
                        default:
                            Logger.WriteLine(statusMessage);
                            break;
                    }
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
            if (buildType.IsPaused)
            {
                Logger.Verbose("Bypassing because it is paused");
                return null;
            }

            string branchLocator = teamcityConfig.IncludeAllBranches
                ? "branch:default:any"
                : "branch:(default:any,policy:active_history_and_active_vcs_branches)";

            string failedToStartLocator = teamcityConfig.IncludeFailedToStart ? "failedToStart:any" : "failedToStart:false";
            DateTimeOffset fromDate = DateTimeOffset.Now.Subtract(teamcityConfig.TimeSpan);
            string fromDateInTcFormat = Uri.EscapeDataString(fromDate.ToString("yyyyMMdd'T'HHmmssK").Replace(":", ""));
            string locator = $"{branchLocator},{failedToStartLocator},running:false,sinceDate:{fromDateInTcFormat}";
            string buildsXml = await httpClient.GetStringAsync(
                $"{authPath}/app/rest/buildTypes/id:{buildType.Id}/builds?locator={locator}");

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

                if (teamcityConfig.OnlyDefaultBranch && !build.DefaultBranch)
                    continue;
                
                if (!dictionary.ContainsKey(branch) || dictionary[branch].Id < build.Id)
                {
                    dictionary[branch] = build;
                }
            }

            Build firstFailingBuild = dictionary.Values.FirstOrDefault(v => !v.IsSuccessful);
            if (firstFailingBuild == null)
            {
                return BuildStatus.Passed;
            }

            Logger.Verbose($"Build from branch {firstFailingBuild.BranchName} (id: {firstFailingBuild.Id}) failed");

            if (buildType.IsUnstable)
            {
                Logger.Verbose("Bypassing because it is marked as unstable.");
                return null;
            }

            BuildStatus? status = BuildStatus.Failed;

            Logger.Verbose("Now checking investigation status.");

            string investigationsXml =
                await httpClient.GetStringAsync($"/{authPath}/app/rest/investigations?locator=buildType:(id:{buildType.Id})");

            var investigation = Investigation.FromXml(investigationsXml);

            return investigation.Status ?? status;
        }
    }
}