using System;
using System.Globalization;

using Beacon.Core;
using Beacon.Lights;

using CommandLine;

namespace Beacon
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var options = new Options();
            var parser = new Parser(with =>
            {
                with.CaseSensitive = false;
                with.HelpWriter = Console.Error;
                with.MutuallyExclusive = true;
                with.ParsingCulture = CultureInfo.InvariantCulture;
            });

            if (parser.ParseArguments(args, options))
            {
                Logger.VerboseEnabled = options.Verbose;

                IBuildLight buildLight = new LightFactory().CreateLight(options.Device);

                var config = new Config
                {
                    ServerUrl = options.Url,
                    Username = options.Username,
                    Password = options.Password,
                    Interval = TimeSpan.FromSeconds(int.Parse(options.IntervalInSeconds)),
                    TimeSpan = TimeSpan.FromDays(int.Parse(options.Timespan)),
                    BuildTypeIds = string.Join(",", options.BuildTypeIds),
                    RunOnce = options.RunOnce,
                    GuestAccess = options.GuestAccess
                };

                new TeamCityMonitor(config, buildLight).Start().Wait();
            }
        }
    }
}