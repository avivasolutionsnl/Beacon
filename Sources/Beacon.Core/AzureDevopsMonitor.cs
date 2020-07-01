using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Beacon.Core
{
    public class AzureDevopsMonitor
    {
        private readonly IBuildLight light;
        private readonly AzureDevopsConfig config;
        private readonly BuildHttpClient buildClient;

        public AzureDevopsMonitor(AzureDevopsConfig config, IBuildLight light)
        {
            this.config = config;
            this.light = light;
            
            var credentials = config.PersonalAccessToken == null ? new VssCredentials() : new VssBasicCredential(string.Empty, config.PersonalAccessToken);
            VssConnection connection = new VssConnection(config.Url, credentials);

            buildClient = connection.GetClient<BuildHttpClient>();
        }

        public async Task Start()
        {
            Console.CancelKeyPress += delegate { light.NoStatus(); };
            
            var definitions = new[] { (await buildClient.GetDefinitionAsync(config.ProjectName, config.DefinitionId)).Id };

            do
            {
                try
                {
                    var build = (await buildClient.GetBuildsAsync(config.ProjectName, definitions, top: 1,
                        branchName: config.BranchName)).FirstOrDefault();

                    if (build.Status == Microsoft.TeamFoundation.Build.WebApi.BuildStatus.Completed)
                    {
                        switch (build.Result)
                        {
                            case BuildResult.Succeeded:
                                light.Success();
                                Logger.WriteLine("Passed");
                                break;

                            case BuildResult.PartiallySucceeded:
                                light.Investigate();
                                Logger.WriteLine("Investigating");
                                break;

                            case BuildResult.Failed:
                                light.Fail();
                                Logger.WriteLine("Failed");
                                break;

                            case BuildResult.Canceled:
                                light.Investigate();
                                Logger.WriteLine("Cancelled");
                                break;

                            default:
                                light.NoStatus();
                                Logger.WriteLine("Build status not available");
                                break;
                        }
                    }

                    if (!config.RunOnce)
                    {
                        Logger.Verbose($"Waiting for {config.Interval} seconds.");
                        await Task.Delay(config.Interval);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
                
            }
            while (!config.RunOnce);
        }
    }
}