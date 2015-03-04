using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Beacon.Core
{
    // REFACTOR the evaluation of multiple statusses and driving the build light should move out of this class, so we can define
    // an abstraction against TeamCity that we can replace with TFS
    public class TeamCityMonitor
    {
        public void Start(Config config, string password, IBuildLight buildLight)
        {
            var buildTypeIds = config.BuildTypeIds == "*"
                ? new string[0]
                : config.BuildTypeIds.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).ToArray();

            buildLight.NoStatus();

            Console.CancelKeyPress += delegate { buildLight.NoStatus(); };

            while (true)
            {
                BuildStatus lastBuildStatus = GetBuildStatus(config.ServerUrl, config.Username, password, buildTypeIds);

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

                Wait();
            }
        }

        private static void Wait()
        {
            Logger.Verbose("Waiting for 10 seconds.");

            var delayCount = 0;
            while (delayCount < 10 && !Console.KeyAvailable)
            {
                delayCount++;
                Thread.Sleep(1000);
            }
        }

        private static BuildStatus GetBuildStatus(string serverUrl, string username, string password,
            IEnumerable<string> buildTypeIds)
        {
            BuildStatus status = BuildStatus.Unavailable;

            try
            {
                List<BuildStatus> results = GetStatusOfAllBuilds(serverUrl, username, password, buildTypeIds.ToArray());
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

        private static List<BuildStatus> GetStatusOfAllBuilds(string serverUrl, string username, string password,
            IEnumerable<string> buildTypeIds)
        {
            var statusPerBuild = new List<BuildStatus>();

            dynamic query = new Query(serverUrl, username, password);

            bool couldFindProjects = false;

            foreach (var project in query.Projects)
            {
                couldFindProjects = true;
                if (project.BuildTypesExists)
                {
                    foreach (var buildType in project.BuildTypes)
                    {
                        if (buildTypeIds.Any() && buildTypeIds.Any(id => id == buildType.Id))
                        {
                            BuildStatus? status = GetBuildTypeStatus(serverUrl, username, password, project, buildType);
                            if (status.HasValue)
                            {
                                statusPerBuild.Add(status.Value);
                            }
                        }
                    }
                }
            }

            if (!couldFindProjects)
            {
                Logger.Verbose(
                    "No Projects found! Please ensure if TeamCity URL is valid and also TeamCity setup and credentials are correct.");
            }

            return statusPerBuild;
        }

        private static BuildStatus? GetBuildTypeStatus(string serverUrl, string username, string password, dynamic project,
            dynamic buildType)
        {
            Logger.Verbose("Analyzing Built Type '{0}\\{1}'.", project.Name, buildType.Name);

            BuildStatus? status = null;

            if (buildType.PausedExists && "true".Equals(buildType.Paused, StringComparison.CurrentCultureIgnoreCase))
            {
                Logger.Verbose("Bypassing because it is paused");

                return null;
            }

            var builds = buildType.Builds;
            var latestBuild = builds.First;
            if (latestBuild == null)
            {
                Logger.Verbose("Bypassing because no built history is available for it yet.");

                return null;
            }

            if ("success".Equals(latestBuild.Status, StringComparison.CurrentCultureIgnoreCase))
            {
                // NOTE Bypassing running build detection to make sure failing builds don't affect the build light until it has compelted
                //                dynamic runningBuild = new Query(serverUrl, username, password)
                //                {
                //                    RestBasePath = string.Format("/httpAuth/app/rest/buildTypes/id:{0}/builds/running:any", buildType.Id)
                //                };

                //                runningBuild.Load();

                //                if ("success".Equals(runningBuild.Status, StringComparison.CurrentCultureIgnoreCase))
                //                {
                //                    Logger.Verbose(
                //                        "Bypassing because status of last build and all running builds is success.");
                //            
                //                    return BuildStatus.Passed;
                //                }

                return BuildStatus.Passed;
            }

            if (latestBuild.PropertiesExists)
            {
                bool isUnstableBuild = false;
                foreach (var property in latestBuild.Properties)
                {
                    if ("system.BuildState".Equals(property.Name, StringComparison.CurrentCultureIgnoreCase) &&
                        "unstable".Equals(property.Value, StringComparison.CurrentCultureIgnoreCase))
                    {
                        isUnstableBuild = true;
                    }

                    if ("BuildState".Equals(property.Name, StringComparison.CurrentCultureIgnoreCase) &&
                        "unstable".Equals(property.Value, StringComparison.CurrentCultureIgnoreCase))
                    {
                        isUnstableBuild = true;
                    }
                }

                if (isUnstableBuild)
                {
                    Logger.Verbose("Bypassing because it is marked as unstable.");

                    return null;
                }
            }

            Logger.Verbose("Now checking investigation status.");
            var buildId = buildType.Id;
            dynamic investigationQuery = new Query(serverUrl, username, password);
            investigationQuery.RestBasePath = @"/httpAuth/app/rest/buildTypes/id:" + buildId + @"/";

            status = BuildStatus.Failed;

            foreach (var investigation in investigationQuery.Investigations)
            {
                string investigationState = investigation.State;

                if ("taken".Equals(investigationState, StringComparison.CurrentCultureIgnoreCase))
                {
                    Logger.Verbose("Investigation status detected as 'taken'");

                    status = BuildStatus.Investigating;
                }

                if ("fixed".Equals(investigationState, StringComparison.CurrentCultureIgnoreCase))
                {
                    Logger.Verbose("Investigation status detected as 'fixed'.");

                    status = BuildStatus.Fixed;
                }
            }

            Logger.Verbose("Concluding status as {0}.", status);

            return status;
        }
    }
}