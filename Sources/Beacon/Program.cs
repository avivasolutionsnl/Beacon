using System;
using System.Collections.Generic;
using System.Reflection;

using Beacon.Core;
using Beacon.Lights;
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

            var result = parser.ParseArguments<Options>(args);
            result.WithParsed(RunTeamCityMonitor).WithNotParsed(errors => HandleParseErrors(result, errors));

            if (result.Tag.Equals(ParserResultType.NotParsed))
            {
                return (int) ExitCodes.Error;
            }

            return (int) ExitCodes.Success;
        }

        private static void RunTeamCityMonitor(Options options)
        {
            var buildLight = new LightFactory().CreateLight(options.Device);
            var config = new Config
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

        private static void HandleParseErrors(ParserResult<Options> result, IEnumerable<Error> errors)
        {
            var helpText = new HelpText
            {
                AddDashesToOption = true,
                AdditionalNewLineAfterOption = true,
                Copyright = new CopyrightInfo("Dennis Doomen", 2015, 2018).ToString(),
                Heading = new HeadingInfo("Beacon: TeamCity Monitor", Assembly.GetExecutingAssembly().GetName().Version.ToString())
            };

            helpText.AddOptions(result);

            var newHelpText = HelpText.AutoBuild(result,
                onError => HelpText.DefaultParsingErrorsHandler(result, helpText), example => example);

            Console.Error.WriteLine(newHelpText);
        }
    }
}