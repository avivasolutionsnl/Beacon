using System.Collections.Generic;

using CommandLine;
using CommandLine.Text;

namespace Beacon
{
    internal class Options
    {
        [Option("url", Required = true, HelpText = "The root URL of the TeamCity server.")]
        public string Url { get; set; }

        [Option("username", Required = true,
            HelpText = 
                "The username of the account that has read access to TeamCity. Options username/password and guestaccess are mutually exclusive.", 
            SetName = "auth")]
        public string Username { get; set; }

        [Option("password", Required = true,
            HelpText = "The password of the account that has read access to TeamCity.", SetName = "auth")]
        public string Password { get; set; }

        [Option("builds", Required = true, 
            HelpText = "One or more builds identified by their TeamCity id (eg, bt64 bt12 or * for all).")]
        public IEnumerable<string> BuildTypeIds { get; set; }

        [Option("device", Default = "delcom", 
            HelpText = "The device to use as the build light (e.g. console, delcom).")]
        public string Device { get; set; }

        [Option("interval", Default = 10, 
            HelpText = "The interval in seconds at which to check the build status.")]
        public int IntervalInSeconds { get; set; }

        [Option("timespan", Default = 7, HelpText = "The timespan in days to include builds from.")]
        public int Timespan { get; set; }

        [Option('a', "all", Default = false, HelpText = "Monitor builds for all branches, not just active branches.")]
        public bool IncludeAllBranches { get; set; }
        
        [Option('f', "failed", Default = false, HelpText = "Monitor builds that failed to start as well, not just finished builds.")]
        public bool IncludeFailedToStart { get; set; }

        [Option('g', "guestaccess", Required = true,
            HelpText = 
                "Check the build status using the TeamCity guest account. Options guestaccess and username/password are mutually exclusive.", 
            SetName = "guest")]
        public bool GuestAccess { get; set; }

        [Option('r', "runonce", Default = false, HelpText = "Check the build status only once.")]
        public bool RunOnce { get; set; }

        [Option('v', "verbose", HelpText = "Log verbose messages.")]
        public bool Verbose { get; set; }
        
        [Option('d', "defaultbranch", Default = false, HelpText = "Only monitor the status of the default branch")]
        public bool OnlyDefaultBranch { get; set; }

        [Usage(ApplicationAlias = "Beacon.exe")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                var unParserSettings = new UnParserSettings
                {
                    GroupSwitches = true
                };

                yield return new Example("Using TeamCity credentials", unParserSettings, new Options
                {
                    BuildTypeIds = new[] { "SomeBuildId", "SomeOtherBuildId" },
                    Password = "pass",
                    Url = "http://teamcity.local",
                    Username = "user"
                });

                yield return new Example("Using TeamCity guest access and running only once before exiting", unParserSettings, new Options
                {
                    BuildTypeIds = new[] { "SomeBuildId", "SomeOtherBuildId" },
                    GuestAccess = true,
                    RunOnce = true,
                    Url = "http://teamcity.local"
                });

                yield return new Example("Using TeamCity guest access, running only once before exiting and verbose logging", unParserSettings, new Options
                {
                    BuildTypeIds = new[] { "SomeBuildId", "SomeOtherBuildId" },
                    GuestAccess = true,
                    RunOnce = true,
                    Url = "http://teamcity.local",
                    Verbose = true
                });
            }
        }
    }
}