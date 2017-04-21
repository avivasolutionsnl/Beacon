using System;

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

            if (Parser.Default.ParseArguments(args, options))
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
                    BuildTypeIds = string.Join(",", options.BuildTypeIds)
                };

                new TeamCityMonitor(config, buildLight).Start().Wait();
            }
        }
    }
}