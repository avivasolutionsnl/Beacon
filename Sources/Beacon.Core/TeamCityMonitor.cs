using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Beacon.Core
{
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
                var lastBuildStatus = RetrieveBuildStatus(config.ServerUrl, config.Username, password, buildTypeIds);

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

        private static BuildStatus RetrieveBuildStatus(string serverUrl, string username, string password,
            IEnumerable<string> buildTypeIds)
        {
            Logger.Verbose("Checking build status.");

            buildTypeIds = buildTypeIds.ToArray();

            dynamic query = new Query(serverUrl, username, password);

            var buildStatus = BuildStatus.Passed;

            try
            {
                var couldFindProjects = false;
                foreach (var project in query.Projects)
                {
                    couldFindProjects = true;
                    Logger.Verbose("Checking Project '{0}'.", project.Name);
                    if (!project.BuildTypesExists)
                    {
                        Logger.Verbose("Bypassing Project '{0}' because it has no 'BuiltTypes' property defined.", project.Name);
                        continue;
                    }

                    foreach (var buildType in project.BuildTypes)
                    {
                        Logger.Verbose("Checking Built Type '{0}\\{1}'.", project.Name, buildType.Name);
                        if (buildTypeIds.Any() && buildTypeIds.All(id => id != buildType.Id))
                        {
                            Logger.Verbose(
                                "Bypassing Built Type '{0}\\{1}' because it does NOT match configured built-type list to monitor.",
                                project.Name, buildType.Name);
                            continue;
                        }

                        if (buildType.PausedExists && "true".Equals(buildType.Paused, StringComparison.CurrentCultureIgnoreCase))
                        {
                            Logger.Verbose("Bypassing Built Type '{0}\\{1}' because it has 'Paused' property set to 'true'.",
                                project.Name, buildType.Name);
                            continue;
                        }

                        var builds = buildType.Builds;
                        var latestBuild = builds.First;
                        if (latestBuild == null)
                        {
                            Logger.Verbose("Bypassing Built Type '{0}\\{1}' because no built history is available to it yet.",
                                project.Name, buildType.Name);
                            continue;
                        }

                        if ("success".Equals(latestBuild.Status, StringComparison.CurrentCultureIgnoreCase))
                        {
                            dynamic runningBuild = new Query(serverUrl, username, password)
                            {
                                RestBasePath =
                                    string.Format("/httpAuth/app/rest/buildTypes/id:{0}/builds/running:any", buildType.Id)
                            };

                            runningBuild.Load();
                            if ("success".Equals(runningBuild.Status, StringComparison.CurrentCultureIgnoreCase))
                            {
                                Logger.Verbose(
                                    "Bypassing Built Type '{0}\\{1}' because status of last build and all running builds are 'success'.",
                                    project.Name, buildType.Name);
                                continue;
                            }
                        }

                        if (latestBuild.PropertiesExists)
                        {
                            var isUnstableBuild = false;
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
                                Logger.Verbose("Bypassing Built Type '{0}\\{1}' because it is marked as 'unstable'.", project.Name,
                                    buildType.Name);
                                continue;
                            }
                        }

                        Logger.Verbose("Now checking investigation status of Built Type '{0}\\{1}'.", project.Name, buildType.Name);
                        var buildId = buildType.Id;
                        dynamic investigationQuery = new Query(serverUrl, username, password);
                        investigationQuery.RestBasePath = @"/httpAuth/app/rest/buildTypes/id:" + buildId + @"/";
                        buildStatus = BuildStatus.Failed;

                        foreach (var investigation in investigationQuery.Investigations)
                        {
                            var investigationState = investigation.State;
                            if ("taken".Equals(investigationState, StringComparison.CurrentCultureIgnoreCase) ||
                                "fixed".Equals(investigationState, StringComparison.CurrentCultureIgnoreCase))
                            {
                                Logger.Verbose(
                                    "Investigation status of Built Type '{0}\\{1}' detected as either 'taken' or 'fixed'.",
                                    project.Name, buildType.Name);
                                buildStatus = BuildStatus.Investigating;
                            }
                        }

                        if (buildStatus == BuildStatus.Failed)
                        {
                            Logger.Verbose("Concluding status of Built Type '{0}\\{1}' as FAIL.", project.Name, buildType.Name);
                            return BuildStatus.Failed;
                        }
                    }
                }

                if (!couldFindProjects)
                {
                    Logger.Verbose(
                        "No Projects found! Please ensure if TeamCity URL is valid and also TeamCity setup and credentials are correct.");
                    return BuildStatus.Unavailable;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                return BuildStatus.Unavailable;
            }

            return buildStatus;
        }
    }
}