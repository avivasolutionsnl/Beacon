using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Threading.Tasks;

using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Common;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Beacon.Core
{
    public class AzureDevOpsMonitor : BuildMonitor
    {
        private readonly AzureDevOpsConfig azureDevOpsConfig;
        private readonly BuildHttpClient buildClient;
        private IEnumerable<int> buildIds;

        public AzureDevOpsMonitor(AzureDevOpsConfig config, IBuildLight light) : base(light, config)
        {
            this.azureDevOpsConfig = config;
            
            var credentials = config.PersonalAccessToken == null ? new VssCredentials() : new VssBasicCredential(string.Empty, config.PersonalAccessToken);
            VssConnection connection = new VssConnection(config.Url, credentials);

            buildClient = connection.GetClient<BuildHttpClient>();
            
            buildIds = config.BuildTypeIds == "*"
                ? new int[0]
                : config.BuildTypeIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => Int32.Parse(id)).ToArray();
        }

        protected override async Task<BuildStatus> GetBuildStatus()
        {
            // Select all build definitions
            if (buildIds.IsNullOrEmpty())
            {
                var definitions = await buildClient.GetDefinitionsAsync(azureDevOpsConfig.ProjectName);
                buildIds = definitions.Select(d => d.Id);
            }

            var statuses = await GetBuildStatuses();

            return CombineStatuses(statuses);
        }

        private async Task<List<BuildStatus>> GetBuildStatuses()
        {
            var statusPerBuild = new List<BuildStatus>();

            foreach (var buildId in buildIds)
            {
                try
                {
                    var build = (await buildClient.GetBuildsAsync(azureDevOpsConfig.ProjectName, new[] { buildId },
                        top: 1)).FirstOrDefault();

                    if (build != null && build.Status == Microsoft.TeamFoundation.Build.WebApi.BuildStatus.Completed && build.Result != null)
                    {
                        statusPerBuild.Add(BuildResultToStatus(build.Result.Value));
                    }
                    else
                    {
                        Logger.WriteErrorLine("Did not receive build info");
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
            
            return statusPerBuild;
        }

        private Beacon.Core.BuildStatus BuildResultToStatus(BuildResult result)
        {
            switch (result)
            {
                case BuildResult.Succeeded:
                    return BuildStatus.Passed;

                case BuildResult.PartiallySucceeded:
                    return BuildStatus.Investigating;

                case BuildResult.Failed:
                    return BuildStatus.Failed;

                case BuildResult.Canceled:
                    return BuildStatus.Investigating;
                
                default:
                    return BuildStatus.Unavailable;
            }
        }
    }
}