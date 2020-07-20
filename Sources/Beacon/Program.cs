using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using Beacon.Core;
using Beacon.Lights;
using Beacon.Lights.Shelly;

using CommandLine;
using CommandLine.Text;

namespace Beacon
{
    [Flags]
    internal enum ExitCodes
    {
        Success = 0,
        Error = 1
    }

    internal class Program
    {
        public static int Main(string[] args)
        {
            var parser = new Parser(with =>
            {
                with.HelpWriter = null;
            });

            var result = parser.ParseArguments<AzureDevOpsOptions, TeamcityOptions>(args);
            
            result.WithParsed<TeamcityOptions>(RunTeamCityMonitor)
                .WithParsed<AzureDevOpsOptions>(RunAzureDevopsMonitor)
                .WithNotParsed(errors => HandleParseErrors(result, errors));

            if (result.Tag.Equals(ParserResultType.NotParsed))
            {
                return (int) ExitCodes.Error;
            }

            return (int) ExitCodes.Success;
        }

        private static void RunTeamCityMonitor(TeamcityOptions options)
        {
            var buildLight = new LightFactory().CreateLight(options.Device);
            if (buildLight is ShellyLight)
            {
                (buildLight as ShellyLight).Configure(options.ShellyUrl);
            }
            var config = new TeamcityConfig
            {
                ServerUrl = options.Url,
                Username = options.Username,
                Password = options.Password,
                Interval = TimeSpan.FromSeconds(options.IntervalInSeconds),
                TimeSpan = TimeSpan.FromDays(options.Timespan),
                BuildTypeIds = string.Join(",", options.BuildTypeIds),
                RunOnce = options.RunOnce,
                GuestAccess = options.GuestAccess,
                IncludeAllBranches = options.IncludeAllBranches,
                IncludeFailedToStart = options.IncludeFailedToStart,
                OnlyDefaultBranch = options.OnlyDefaultBranch,
            };

            Logger.VerboseEnabled = options.Verbose;
            new TeamCityMonitor(config, buildLight).Start().Wait();
        }

        private static void RunAzureDevopsMonitor(AzureDevOpsOptions options)
        {
            var buildLight = new LightFactory().CreateLight(options.Device);
            if (buildLight is ShellyLight)
            {
                (buildLight as ShellyLight).Configure(options.ShellyUrl);
            }
            var config = new AzureDevOpsConfig()
            {
                Url = new Uri(options.Url),
                BuildTypeIds = string.Join(",", options.BuildTypeIds),
                Interval = TimeSpan.FromSeconds(options.IntervalInSeconds),
                ProjectName = options.ProjectName,
                PersonalAccessToken = options.PersonalAccessToken,
                RunOnce = options.RunOnce
            };
            
            Logger.VerboseEnabled = options.Verbose;
            new AzureDevOpsMonitor(config, buildLight).Start().Wait();
        }
        
        private static void HandleParseErrors(ParserResult<object> result, IEnumerable<Error> errors)
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
           
            var helpText = new HelpText
            {
                AddDashesToOption = true,
                AdditionalNewLineAfterOption = true,
                Copyright = versionInfo.LegalCopyright,
                Heading = new HeadingInfo("Beacon: TeamCity Monitor", Assembly.GetExecutingAssembly().GetName().Version.ToString())
            };

            helpText.AddOptions(result);

            var newHelpText = HelpText.AutoBuild(result,
                onError => HelpText.DefaultParsingErrorsHandler(result, helpText), example => example);

            Console.Error.WriteLine(newHelpText);
        }
    }
}