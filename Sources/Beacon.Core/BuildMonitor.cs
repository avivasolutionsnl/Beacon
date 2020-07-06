using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beacon.Core
{
    public abstract class BuildMonitor
    {
        protected readonly IBuildLight light;
        protected readonly Config config;
        
        protected BuildMonitor(IBuildLight light, Config config)
        {
            this.light = light;
            this.config = config;
        }
        
        public async Task Start()
        {
            Console.CancelKeyPress += delegate { light.NoStatus(); };

            do
            {
                BuildStatus lastBuildStatus = await GetBuildStatus();

                switch (lastBuildStatus)
                {
                    case BuildStatus.Unavailable:
                        light.NoStatus();
                        Logger.WriteLine("Build status not available");
                        break;

                    case BuildStatus.Passed:
                        light.Success();
                        Logger.WriteLine("Passed");
                        break;

                    case BuildStatus.Investigating:
                        light.Investigate();
                        Logger.WriteLine("Investigating");
                        break;

                    case BuildStatus.Failed:
                        light.Fail();
                        Logger.WriteLine("Failed");
                        break;

                    case BuildStatus.Fixed:
                        light.Fixed();
                        Logger.WriteLine("Fixed");
                        break;
                }

                if (!config.RunOnce)
                {
                    Logger.Verbose($"Waiting for {config.Interval} seconds.");
                    await Task.Delay(config.Interval);
                }
            } while (!config.RunOnce);
        }
        
        protected BuildStatus CombineStatuses(IEnumerable<BuildStatus> statuses)
        {
            BuildStatus status = BuildStatus.Unavailable;

            if (!statuses.Any())
            {
                status = BuildStatus.Unavailable;
            }
            else if (statuses.Any(result => result == BuildStatus.Failed))
            {
                status = BuildStatus.Failed;
            }
            else if (statuses.Any(result => result == BuildStatus.Investigating))
            {
                status = BuildStatus.Investigating;
            }
            else
            {
                status = BuildStatus.Passed;
            }

            return status;
        }

        protected abstract Task<BuildStatus> GetBuildStatus();
    }
}